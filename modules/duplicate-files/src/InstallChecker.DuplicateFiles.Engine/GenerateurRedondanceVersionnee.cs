using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles;

public static class GenerateurRedondanceVersionnee
{
    private static readonly IReadOnlyList<IFournisseurDePreuves> FournisseursParDefaut =
    [
        new FournisseurNomDeFichier(),
        new FournisseurVersionInfo(),
        new FournisseurMsi(),
        new FournisseurAppx(),
        new FournisseurPe(),
        new FournisseurAuthenticode(),
    ];

    public static RapportRedondanceVersionnee Generer(
        IObservationsSource omega,
        IReadOnlyList<IFournisseurDePreuves>? fournisseurs = null)
    {
        ArgumentNullException.ThrowIfNull(omega);
        fournisseurs ??= FournisseursParDefaut;

        var fichiers = ProjectionFichiersObserves.Projeter(omega);
        var exclusions = Enum.GetValues<MotifExclusionVersionnee>()
            .ToDictionary(motif => motif, _ => 0);
        var artefacts = fichiers
            .GroupBy(f => f.ContenuSha256, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => ResoudreContenu(g.Key, g.ToList(), fournisseurs))
            .ToList();

        var comparables = new List<ArtefactVersionne>();
        foreach (var artefact in artefacts)
        {
            var motif = MotifExclusion(artefact);
            if (motif is null)
                comparables.Add(artefact);
            else
                exclusions[motif.Value]++;
        }

        var groupes = new List<GroupeRedondanceVersionnee>();
        foreach (var famille in comparables
            .GroupBy(a => new CleFamilleVersionnee(
                a.SourceFamille!.Value,
                a.CleFamille!,
                a.Version!.Value.Schema))
            .OrderBy(g => g.Key.Cle, StringComparer.Ordinal)
            .ThenBy(g => g.Key.Schema))
        {
            var variantes = famille
                .GroupBy(a => a.Variante)
                .OrderBy(g => g.Key.Format, StringComparer.Ordinal)
                .ThenBy(g => g.Key.Architecture ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(g => g.Key.Langue ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(g => g.Key.Edition ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(g => g.Key.Distribution ?? string.Empty, StringComparer.Ordinal)
                .ToList();
            var plusieursVariantes = variantes.Count > 1;
            var plusieursVersionsDansLaFamille = famille
                .Select(a => a.Version!.Value)
                .Distinct()
                .Count() > 1;

            foreach (var variante in variantes)
            {
                var contenus = variante
                    .OrderBy(a => a.Version!.Value)
                    .ThenBy(a => a.ContenuSha256, StringComparer.Ordinal)
                    .ToList();
                if (contenus.Select(a => a.Version!.Value).Distinct().Count() < 2)
                {
                    var motif = plusieursVariantes && plusieursVersionsDansLaFamille
                        ? MotifExclusionVersionnee.VarianteIncompatible
                        : MotifExclusionVersionnee.MemeVersionSeulement;
                    exclusions[motif] += contenus.Count;
                    continue;
                }

                groupes.Add(CreerGroupe(famille.Key, variante.Key, contenus));
            }
        }

        var groupesTries = groupes
            .OrderBy(g => g.Famille, StringComparer.Ordinal)
            .ThenBy(g => g.Variante.Format, StringComparer.Ordinal)
            .ThenBy(g => g.Variante.Architecture ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(g => g.Variante.Langue ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(g => g.GroupeId, StringComparer.Ordinal)
            .ToList();
        var artefactsPublies = groupesTries.SelectMany(g => g.Artefacts).ToList();

        return new RapportRedondanceVersionnee(
            VersionsContratDuplicateFiles.RedondanceVersionneeV1,
            new SourceRapportVersionnee(fichiers.Count),
            new SyntheseRedondanceVersionnee(
                groupesTries.Count,
                artefactsPublies.Count,
                artefactsPublies.Count(a => a.Role == RoleComparaisonVersionnee.ReferenceRecente),
                artefactsPublies.Count(a => a.Role == RoleComparaisonVersionnee.VersionAnterieure),
                artefacts.Count(a => a.Etat is EtatResolutionVersionnee.ConflitDeFamille or
                    EtatResolutionVersionnee.ConflitDeVersion)),
            groupesTries,
            exclusions);
    }

    private static ArtefactVersionne ResoudreContenu(
        string contenuSha256,
        IReadOnlyList<FichierObserve> fichiers,
        IReadOnlyList<IFournisseurDePreuves> fournisseurs)
    {
        var preuves = new List<PreuveVersionnee>();
        var diagnostics = new List<DiagnosticVersionne>();
        foreach (var fichier in fichiers.OrderBy(f => f.FichierId, StringComparer.Ordinal))
        {
            foreach (var fournisseur in fournisseurs)
            {
                var resultat = fournisseur.Extraire(fichier);
                preuves.AddRange(resultat.Preuves);
                diagnostics.AddRange(resultat.Diagnostics);
            }
        }

        return ResolveurArtefactVersionne.Resoudre(contenuSha256, fichiers, preuves, diagnostics);
    }

    private static MotifExclusionVersionnee? MotifExclusion(ArtefactVersionne artefact)
    {
        if (artefact.Etat == EtatResolutionVersionnee.ConflitDeFamille)
            return MotifExclusionVersionnee.ConflitDeFamille;
        if (artefact.Etat == EtatResolutionVersionnee.ConflitDeVersion)
            return MotifExclusionVersionnee.ConflitDeVersion;
        if (artefact.CleFamille is null || artefact.SourceFamille is null)
            return MotifExclusionVersionnee.FamilleInsuffisante;
        if (artefact.Version is not null)
            return null;
        return artefact.Diagnostics.Any(d => d.Code == CodeDiagnosticVersionne.VersionNonComparable)
            ? MotifExclusionVersionnee.VersionNonComparable
            : MotifExclusionVersionnee.AucuneVersion;
    }

    private static GroupeRedondanceVersionnee CreerGroupe(
        CleFamilleVersionnee famille,
        VarianteVersionnee variante,
        IReadOnlyList<ArtefactVersionne> contenus)
    {
        var reference = contenus.Max(a => a.Version!.Value);
        var confiance = contenus.Min(a => a.Confiance);
        if (EditeurSigneCommun(contenus))
            confiance = Augmenter(confiance);
        if (variante.Partielle && confiance == NiveauConfianceVersionnee.Forte)
            confiance = NiveauConfianceVersionnee.Moyenne;

        var blocagesGroupe = Blocages(confiance, variante.Partielle);
        var artefacts = contenus.Select(a => new ArtefactVersionneRapporte(
            a.ContenuSha256,
            a.Fichiers.Select(f => new FichierVersionneRapporte(f.FichierId, f.Chemin, f.Taille)).ToList(),
            a.Version!.Value.Canonique,
            a.Version.Value.CompareTo(reference) == 0
                ? RoleComparaisonVersionnee.ReferenceRecente
                : RoleComparaisonVersionnee.VersionAnterieure,
            a.Preuves,
            a.Diagnostics,
            [ActionVersionnee.Examiner, ActionVersionnee.Ignorer],
            Blocages(a.Confiance, a.Variante.Partielle))).ToList();

        return new GroupeRedondanceVersionnee(
            IdentifiantsStables.PourGroupeVersionne(
                famille.Source.ToString().ToLowerInvariant(),
                famille.Cle,
                famille.Schema,
                variante.Format,
                variante.Architecture,
                variante.Langue,
                variante.Edition,
                variante.Distribution),
            CategorieRedondanceVersionnee.VersionRedundancyCandidate,
            contenus[^1].LibelleFamille ?? famille.Cle,
            variante,
            confiance,
            reference.Canonique,
            artefacts,
            blocagesGroupe);
    }

    private static IReadOnlyList<RaisonBlocageVersionnee> Blocages(
        NiveauConfianceVersionnee confiance,
        bool variantePartielle)
    {
        var resultat = new List<RaisonBlocageVersionnee>
        {
            RaisonBlocageVersionnee.RevueHumaineObligatoire,
            RaisonBlocageVersionnee.SuppressionAutomatiqueInterdite,
        };
        if (confiance == NiveauConfianceVersionnee.Faible)
            resultat.Add(RaisonBlocageVersionnee.ConfianceFaible);
        if (variantePartielle)
            resultat.Add(RaisonBlocageVersionnee.VarianteNonObservee);
        return resultat;
    }

    private static bool EditeurSigneCommun(IReadOnlyList<ArtefactVersionne> contenus)
    {
        var sujets = contenus.Select(a => a.Preuves
            .Where(p => p.Source == SourcePreuveVersionnee.Authenticode &&
                p.Dimension == DimensionPreuveVersionnee.Editeur)
            .Select(p => p.ValeurNormalisee)
            .Distinct(StringComparer.Ordinal)
            .ToList()).ToList();
        return sujets.All(s => s.Count == 1) &&
            sujets.Select(s => s[0]).Distinct(StringComparer.Ordinal).Count() == 1;
    }

    private static NiveauConfianceVersionnee Augmenter(NiveauConfianceVersionnee niveau) => niveau switch
    {
        NiveauConfianceVersionnee.Faible => NiveauConfianceVersionnee.Moyenne,
        NiveauConfianceVersionnee.Moyenne => NiveauConfianceVersionnee.Forte,
        _ => NiveauConfianceVersionnee.Forte,
    };

    private sealed record CleFamilleVersionnee(
        SourcePreuveVersionnee Source,
        string Cle,
        SchemaVersionComparable Schema);
}
