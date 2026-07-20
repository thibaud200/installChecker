using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Orchestre les composants du module (plan rev3 § 2-3) : extraction des groupes et des refus des
/// strates supérieures, enrichissement depuis Ω, classement, calcul des métriques par groupe
/// (taille unitaire et espace récupérable — correction B), synthèse de bibliothèque, dérivation de
/// la note de capacité, assemblage. Consomme W et Ω en entrée ; ne produit en sortie que des DTO du
/// module — aucun type moteur (correction P4).
/// </summary>
public static class GenerateurDeRapport
{
    public static RapportDeDoublons Generer(
        W w,
        IObservationsSource omega,
        IReadOnlyDictionary<long, VolumeDuFichier>? volumes = null,
        Func<string, bool>? cheminProtege = null)
    {
        cheminProtege ??= ProtectionDesChemins.EstProtegeParDefaut;
        var (groupesActeW, refusStratesSuperieures) = ExtracteurDeGroupes.Extraire(w);

        var actes = omega.ProjeterModele().Actes.ToDictionary(a => a.Identifiant);
        var contextes = omega.ProjeterContexte().ToDictionary(c => c.Identifiant);

        var groupesClasses = new List<IReadOnlyList<ExemplaireClasse>>();
        var groupes = new List<GroupeClasse>();

        foreach (var groupeActeW in groupesActeW)
        {
            var contenuSha256 = ExtraireContenuSha256(groupeActeW.Domaine, actes);
            var groupeId = IdentifiantsStables.PourGroupeExact(contenuSha256);
            var fichiers = EnrichisseurDeGroupe.Enrichir(groupeActeW.Domaine, actes, contextes, volumes);
            var exemplairesClasses = PolitiqueRetentionV1.Classer(fichiers);
            groupesClasses.Add(exemplairesClasses);

            var tailleUnitaire = exemplairesClasses[0].Fichier.Taille;
            var espaceRecuperable = tailleUnitaire * (long)(exemplairesClasses.Count - 1);

            var exemplaires = exemplairesClasses
                .Select(e => new ExemplaireRapporte(
                    e.Fichier,
                    e.Rang,
                    Etiquette(e.Rang),
                    e.Motif,
                    IdentifiantsStables.PourFichier(contenuSha256, e.Fichier.Chemin),
                    e.Rang == 1 ? RoleExemplaire.RecommandeAConserver : RoleExemplaire.Candidat,
                    PolitiqueRetentionV1.Expliquer(e.Fichier),
                    ConstruireActions(e, cheminProtege)))
                .ToList();

            groupes.Add(new GroupeClasse(
                groupeActeW.Domaine,
                MotifCourt(groupeActeW),
                tailleUnitaire,
                espaceRecuperable,
                exemplaires,
                groupeId,
                CategorieDoublon.ExactDuplicate,
                NiveauConfiance.Certaine,
                contenuSha256,
                [new PreuveDoublon(TypePreuveDoublon.Sha256Identique, contenuSha256)],
                exemplaires[0].FichierId));
        }

        var synthese = SyntheseDeBibliotheque.Calculer(groupesClasses);
        var note = refusStratesSuperieures.Count == 0 ? null : DeriverNote(refusStratesSuperieures);

        return new RapportDeDoublons(
            synthese, note, groupes, VersionsContratDuplicateFiles.DoublonsExactsV1);
    }

    private static IReadOnlyList<EtatActionFichier> ConstruireActions(
        ExemplaireClasse exemplaire,
        Func<string, bool> cheminProtege)
    {
        var blocages = new List<RaisonBlocageAction>();
        if (exemplaire.Rang == 1)
            blocages.Add(RaisonBlocageAction.FichierRecommandeAConserver);
        if (cheminProtege(exemplaire.Fichier.Chemin))
            blocages.Add(RaisonBlocageAction.CheminProtege);

        return
        [
            new EtatActionFichier(ActionFichier.Conserver, true, []),
            new EtatActionFichier(
                ActionFichier.AjouterAuPlanDeSuppression,
                blocages.Count == 0,
                blocages),
        ];
    }

    private static string ExtraireContenuSha256(
        IReadOnlyList<long> domaine,
        IReadOnlyDictionary<long, ActeObservation> actes)
    {
        var empreintes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var acteId in domaine)
        {
            if (!actes.TryGetValue(acteId, out var acte))
                throw new InvalidOperationException($"acte {acteId} absent du snapshot Omega");

            try
            {
                empreintes.Add(IdentifiantsStables.NormaliserSha256(acte.Empreinte));
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"empreinte SHA-256 invalide pour l'acte {acteId}", ex);
            }
        }

        if (empreintes.Count != 1)
            throw new InvalidOperationException("les empreintes du groupe de doublons exacts diffèrent");

        return empreintes.Single();
    }

    private static string Etiquette(int rang) => rang == 1 ? "à conserver" : "candidat à la suppression";

    private static string MotifCourt(ActeW acte) =>
        $"{acte.Motif} ({string.Join(", ", acte.Licences!.Select(l => $"{l.Identifiant} v{l.Version}"))}, niveau {acte.Niveau})";

    private static NoteDeCapacite DeriverNote(IReadOnlyList<ActeW> refusStratesSuperieures)
    {
        var strates = refusStratesSuperieures.Select(r => NomStrate(r.Strate)).Distinct().ToList();
        var message =
            "Regroupement de versions indisponible : aucune convention adoptée dans ℛ au-delà de la strate " +
            $"contenu ({string.Join(", ", strates)} en attente). Cette note disparaîtra dès qu'une convention " +
            "sera adoptée pour ces strates.";
        return new NoteDeCapacite(strates, message);
    }

    private static string NomStrate(Strate strate) => strate switch
    {
        Strate.Contenu => "contenu",
        Strate.Variante => "variante",
        Strate.Version => "version",
        Strate.Identite => "identité",
        Strate.Famille => "famille",
        _ => strate.ToString().ToLowerInvariant(),
    };
}
