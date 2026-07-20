using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class GenerateurDeRapportTests
{
    private const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string HashB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    private sealed class OmegaDeTest(ModeleObservations modele, IReadOnlyList<ContexteObservation> contexte)
        : IObservationsSource
    {
        public ModeleObservations ProjeterModele() => modele;
        public IReadOnlyList<ContexteObservation> ProjeterContexte() => contexte;
        public IndexOmega ProjeterIdentite() => throw new NotSupportedException("non consommé par GenerateurDeRapport");
    }

    private static readonly IndexEtat IndexDeTest = new(new IndexOmega(1, 0, "empreinte-de-test"), []);

    private static ActeW Election(params long[] domaine) => new(
        TypeActe.Election, Strate.Contenu, domaine, "même contenu", Niveau.Certaine, "motif d'élection",
        Espece: null, Licences: [new ConventionRef("CE-01", 1)], Dependances: null, Dette: null);

    private static ActeW RefusStrate(Strate strate) => new(
        TypeActe.Refus, strate, [1, 2], Contenu: null, Niveau: null, "aucune-convention-strate",
        Espece.Normatif, Licences: null, Dependances: null, Dette: null);

    // Deux actes de taille 200 : l'acte 2 porte une signature (plus riche → rang 1).
    private static OmegaDeTest OmegaDeuxFichiers(
        long premierId = 1,
        long secondId = 2,
        string? secondeEmpreinte = null,
        string premierChemin = @"C:\a\setup.exe",
        string secondChemin = @"C:\b\setup.exe") => new(
        new ModeleObservations(
        [
            new ActeObservation(premierId, 200, HashA, new Dictionary<Attribut, ValeurObservee>()),
            new ActeObservation(secondId, 200, secondeEmpreinte ?? HashA, new Dictionary<Attribut, ValeurObservee>
            {
                [new Attribut("authenticode", "subject")] = new ValeurObservee.Texte("Contoso"),
            }),
        ]),
        [
            new ContexteObservation(premierId, premierChemin, "2026-01-01T00:00:00.0000000Z"),
            new ContexteObservation(secondId, secondChemin, "2026-01-01T00:00:00.0000000Z"),
        ]);

    [Fact]
    public void Le_rapport_porte_la_synthese_la_note_de_capacite_et_les_groupes_etiquetes()
    {
        var w = new W(IndexDeTest, [Election(1, 2), RefusStrate(Strate.Variante), RefusStrate(Strate.Version)]);

        var rapport = GenerateurDeRapport.Generer(w, OmegaDeuxFichiers());

        // Synthèse (5 agrégats)
        Assert.Equal(1, rapport.Synthese.NombreDeGroupes);
        Assert.Equal(1, rapport.Synthese.NombreDeFichiersRedondants);
        Assert.Equal(200L, rapport.Synthese.EspaceRecuperableOctets);
        Assert.Equal(1, rapport.Synthese.NombreDeFichiersAConserver);
        Assert.Equal(1, rapport.Synthese.NombreDeCandidatsASuppression);

        // Note de capacité (dérivée des refus des strates supérieures, jamais les refus eux-mêmes)
        Assert.NotNull(rapport.Note);
        Assert.Contains("variante", rapport.Note!.StratesIndisponibles);
        Assert.Contains("version", rapport.Note.StratesIndisponibles);
        Assert.False(string.IsNullOrWhiteSpace(rapport.Note.Message));

        // Groupe : métriques par groupe + exemplaires étiquetés
        var groupe = Assert.Single(rapport.Groupes);
        Assert.Equal([1L, 2L], groupe.Domaine);
        Assert.Contains("CE-01", groupe.MotifCourt);
        Assert.Equal(200L, groupe.TailleUnitaire);
        Assert.Equal(200L, groupe.EspaceRecuperableOctets);
        Assert.Equal(2, groupe.Exemplaires.Count);

        Assert.Equal("duplicate-files/exact-duplicates/v1", rapport.VersionContrat);
        Assert.Equal($"exact:sha256:{HashA}", groupe.GroupeId);
        Assert.Equal(CategorieDoublon.ExactDuplicate, groupe.Categorie);
        Assert.Equal(NiveauConfiance.Certaine, groupe.Confiance);
        Assert.Equal(HashA, groupe.ContenuSha256);
        Assert.Equal(new PreuveDoublon(TypePreuveDoublon.Sha256Identique, HashA), Assert.Single(groupe.Preuves));
        Assert.Equal(groupe.Exemplaires[0].FichierId, groupe.FichierRecommandeId);

        Assert.Equal(2L, groupe.Exemplaires[0].Fichier.ActeId); // le plus riche (signature) classé premier
        Assert.Equal(1, groupe.Exemplaires[0].Rang);
        Assert.Equal("à conserver", groupe.Exemplaires[0].Etiquette);
        Assert.Equal(RoleExemplaire.RecommandeAConserver, groupe.Exemplaires[0].Role);
        Assert.Equal(2, groupe.Exemplaires[1].Rang);
        Assert.Equal("candidat à la suppression", groupe.Exemplaires[1].Etiquette);
        Assert.Equal(RoleExemplaire.Candidat, groupe.Exemplaires[1].Role);
        Assert.Equal(5, groupe.Exemplaires[0].CriteresClassement.Count);
    }

    [Fact]
    public void Les_volumes_fournis_apparaissent_sur_les_exemplaires_et_leur_absence_donne_null()
    {
        var w = new W(IndexDeTest, [Election(1, 2)]);
        var volumes = new Dictionary<long, VolumeDuFichier> { [1] = new("vol-a", "Data") };

        var rapport = GenerateurDeRapport.Generer(w, OmegaDeuxFichiers(), volumes);

        var exemplaires = Assert.Single(rapport.Groupes).Exemplaires;
        var fichier1 = exemplaires.Single(e => e.Fichier.ActeId == 1).Fichier;
        var fichier2 = exemplaires.Single(e => e.Fichier.ActeId == 2).Fichier;
        Assert.Equal("vol-a", fichier1.VolumeId);
        Assert.Equal("Data", fichier1.VolumeLabel);
        Assert.Null(fichier2.VolumeId);   // pas d'entrée volume pour l'acte 2 : absence, jamais une erreur
        Assert.Null(fichier2.VolumeLabel);
    }

    [Fact]
    public void Sans_volumes_le_rapport_reste_celui_daujourdhui()
    {
        var w = new W(IndexDeTest, [Election(1, 2)]);

        var rapport = GenerateurDeRapport.Generer(w, OmegaDeuxFichiers());

        var exemplaires = Assert.Single(rapport.Groupes).Exemplaires;
        Assert.All(exemplaires, e => Assert.Null(e.Fichier.VolumeId));
    }

    [Fact]
    public void La_synthese_precede_les_groupes_dans_le_rapport()
    {
        var parametres = typeof(RapportDeDoublons).GetConstructors()[0].GetParameters().Select(p => p.Name).ToList();

        Assert.True(parametres.IndexOf("Synthese") >= 0);
        Assert.True(parametres.IndexOf("Synthese") < parametres.IndexOf("Groupes"));
    }

    [Fact]
    public void La_note_de_capacite_disparait_quand_aucune_strate_superieure_ne_refuse()
    {
        var w = new W(IndexDeTest, [Election(1, 2)]);

        var rapport = GenerateurDeRapport.Generer(w, OmegaDeuxFichiers());

        Assert.Null(rapport.Note);
    }

    [Fact]
    public void Aucun_type_du_moteur_ne_fuit_dans_la_sortie_du_rapport()
    {
        var types = TypesAtteignables(typeof(RapportDeDoublons), new HashSet<Type>());

        Assert.DoesNotContain(types, t => t.Namespace?.StartsWith("InstallChecker.Identity") == true);
    }

    [Fact]
    public void Les_identifiants_restent_stables_quand_les_ActeId_changent()
    {
        var premier = GenerateurDeRapport.Generer(
            new W(IndexDeTest, [Election(1, 2)]),
            OmegaDeuxFichiers());
        var second = GenerateurDeRapport.Generer(
            new W(IndexDeTest, [Election(11, 12)]),
            OmegaDeuxFichiers(11, 12));

        var groupePremier = Assert.Single(premier.Groupes);
        var groupeSecond = Assert.Single(second.Groupes);
        Assert.Equal(groupePremier.GroupeId, groupeSecond.GroupeId);
        Assert.Equal(
            groupePremier.Exemplaires.OrderBy(e => e.Fichier.Chemin).Select(e => e.FichierId),
            groupeSecond.Exemplaires.OrderBy(e => e.Fichier.Chemin).Select(e => e.FichierId));
    }

    [Fact]
    public void Un_groupe_dont_les_empreintes_different_est_refuse()
    {
        var w = new W(IndexDeTest, [Election(1, 2)]);

        var exception = Assert.Throws<InvalidOperationException>(
            () => GenerateurDeRapport.Generer(w, OmegaDeuxFichiers(secondeEmpreinte: HashB)));

        Assert.Contains("empreintes", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Les_actions_distinguent_le_fichier_recommande_et_le_candidat()
    {
        var rapport = GenerateurDeRapport.Generer(
            new W(IndexDeTest, [Election(1, 2)]),
            OmegaDeuxFichiers());

        var exemplaires = Assert.Single(rapport.Groupes).Exemplaires;
        var recommande = exemplaires.Single(e => e.Role == RoleExemplaire.RecommandeAConserver);
        var candidat = exemplaires.Single(e => e.Role == RoleExemplaire.Candidat);

        Assert.True(Action(recommande, ActionFichier.Conserver).Autorisee);
        var suppressionRecommandee = Action(recommande, ActionFichier.AjouterAuPlanDeSuppression);
        Assert.False(suppressionRecommandee.Autorisee);
        Assert.Equal(
            [RaisonBlocageAction.FichierRecommandeAConserver],
            suppressionRecommandee.Blocages);

        Assert.True(Action(candidat, ActionFichier.Conserver).Autorisee);
        var suppressionCandidate = Action(candidat, ActionFichier.AjouterAuPlanDeSuppression);
        Assert.True(suppressionCandidate.Autorisee);
        Assert.Empty(suppressionCandidate.Blocages);
    }

    [Fact]
    public void Un_candidat_sous_un_chemin_systeme_est_bloque_par_defaut()
    {
        var rapport = GenerateurDeRapport.Generer(
            new W(IndexDeTest, [Election(1, 2)]),
            OmegaDeuxFichiers(premierChemin: @"C:\Windows\setup.exe"));

        var candidatProtege = Assert.Single(rapport.Groupes).Exemplaires
            .Single(e => e.Fichier.Chemin.StartsWith(@"C:\Windows", StringComparison.OrdinalIgnoreCase));
        var suppression = Action(candidatProtege, ActionFichier.AjouterAuPlanDeSuppression);

        Assert.False(suppression.Autorisee);
        Assert.Equal([RaisonBlocageAction.CheminProtege], suppression.Blocages);
    }

    private static EtatActionFichier Action(ExemplaireRapporte exemplaire, ActionFichier action) =>
        exemplaire.Actions.Single(a => a.Action == action);

    private static IReadOnlyList<Type> TypesAtteignables(Type racine, HashSet<Type> vus)
    {
        if (racine.IsGenericParameter || !vus.Add(racine))
            return [];

        var resultat = new List<Type> { racine };
        foreach (var arg in racine.GetGenericArguments())
            resultat.AddRange(TypesAtteignables(arg, vus));

        if (racine.Namespace?.StartsWith("InstallChecker") == true && !racine.IsEnum)
            foreach (var propriete in racine.GetProperties())
                resultat.AddRange(TypesAtteignables(propriete.PropertyType, vus));

        return resultat;
    }
}
