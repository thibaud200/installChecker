using System.Security.Cryptography;
using System.Text;

namespace InstallChecker.DuplicateFiles;

public static class ResolveurArtefactVersionne
{
    private static readonly HashSet<string> FormatsAvecArchitectureRequise = new(
        [".exe", ".msi", ".appx", ".msix", ".appxbundle", ".msixbundle", "<appx-package>"],
        StringComparer.Ordinal);

    public static ArtefactVersionne Resoudre(
        string contenuSha256,
        IReadOnlyList<FichierObserve> fichiers,
        IReadOnlyList<PreuveVersionnee> preuves,
        IReadOnlyList<DiagnosticVersionne> diagnostics)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contenuSha256);
        ArgumentNullException.ThrowIfNull(fichiers);
        ArgumentNullException.ThrowIfNull(preuves);
        ArgumentNullException.ThrowIfNull(diagnostics);
        if (fichiers.Count == 0)
            throw new ArgumentException("au moins un fichier est requis", nameof(fichiers));

        var fichiersTries = fichiers
            .OrderBy(f => IdentifiantsStables.NormaliserCheminWindows(f.Chemin), StringComparer.Ordinal)
            .ToList();
        var preuvesUniques = preuves
            .OrderBy(p => p.FichierId, StringComparer.Ordinal)
            .ThenBy(p => p.Dimension)
            .ThenBy(p => p.Source)
            .ThenBy(p => p.Regle, StringComparer.Ordinal)
            .ThenBy(p => p.ValeurNormalisee, StringComparer.Ordinal)
            .GroupBy(p => (p.Dimension, p.ValeurNormalisee, p.Source, p.Regle))
            .Select(g => g.First())
            .ToList();
        var diagnosticsResultat = diagnostics
            .Distinct()
            .OrderBy(d => d.Code)
            .ThenBy(d => d.Source)
            .ThenBy(d => d.DetailNormalise, StringComparer.Ordinal)
            .ToList();
        var fichierDiagnostic = fichiersTries[0].FichierId;

        var conflitFamille = false;
        string? cleFamille = null;
        string? libelleFamille = null;
        SourcePreuveVersionnee? sourceFamille = null;
        var confiance = NiveauConfianceVersionnee.Faible;

        var clesNatives = ValeursDistinctes(
            preuvesUniques.Where(p => p.Dimension == DimensionPreuveVersionnee.CleFamille));
        if (clesNatives.Count > 1)
        {
            conflitFamille = true;
        }
        else if (clesNatives.Count == 1)
        {
            var preuve = preuvesUniques.First(p =>
                p.Dimension == DimensionPreuveVersionnee.CleFamille &&
                p.ValeurNormalisee == clesNatives[0]);
            cleFamille = preuve.ValeurNormalisee;
            sourceFamille = preuve.Source;
            confiance = NiveauConfianceVersionnee.Forte;
            libelleFamille = preuvesUniques
                .Where(p => p.Dimension == DimensionPreuveVersionnee.LibelleFamille && p.Source == preuve.Source)
                .Select(p => p.ValeurBrute)
                .FirstOrDefault();
        }
        else
        {
            var nomsProduits = ValeursDistinctes(preuvesUniques.Where(p =>
                p.Source == SourcePreuveVersionnee.VersionInfo &&
                p.Regle == "VersionInfoProductName"));
            var societes = ValeursDistinctes(preuvesUniques.Where(p =>
                p.Source == SourcePreuveVersionnee.VersionInfo &&
                p.Regle == "VersionInfoCompanyName"));

            if (nomsProduits.Count > 1 || societes.Count > 1)
            {
                conflitFamille = true;
            }
            else if (nomsProduits.Count == 1 && societes.Count == 1)
            {
                cleFamille = CleComposite("versioninfo-family", nomsProduits[0], societes[0]);
                sourceFamille = SourcePreuveVersionnee.VersionInfo;
                confiance = NiveauConfianceVersionnee.Moyenne;
                libelleFamille = preuvesUniques.First(p =>
                    p.Source == SourcePreuveVersionnee.VersionInfo &&
                    p.Regle == "VersionInfoProductName").ValeurBrute;
            }
            else
            {
                var nomsFichier = ValeursDistinctes(preuvesUniques.Where(p =>
                    p.Source == SourcePreuveVersionnee.NomFichier &&
                    p.Dimension == DimensionPreuveVersionnee.LibelleFamille));
                if (nomsFichier.Count > 1)
                {
                    conflitFamille = true;
                }
                else if (nomsFichier.Count == 1)
                {
                    cleFamille = $"filename-family:{nomsFichier[0]}";
                    sourceFamille = SourcePreuveVersionnee.NomFichier;
                    confiance = NiveauConfianceVersionnee.Faible;
                    libelleFamille = preuvesUniques.First(p =>
                        p.Source == SourcePreuveVersionnee.NomFichier &&
                        p.Dimension == DimensionPreuveVersionnee.LibelleFamille).ValeurBrute;
                }
            }
        }

        var conflitVersion = false;
        VersionComparable? versionResolue = null;
        var versionsComparables = preuvesUniques
            .Where(p => p.Dimension == DimensionPreuveVersionnee.Version)
            .Select(p => (Preuve: p, Lisible: VersionComparable.TryLire(
                p.ValeurNormalisee,
                autoriserPrefixeV: false,
                out var version), Version: version))
            .Where(v => v.Lisible)
            .ToList();

        var structurees = versionsComparables
            .Where(v => v.Preuve.Source is SourcePreuveVersionnee.Msi or SourcePreuveVersionnee.Appx)
            .ToList();
        var productVersions = versionsComparables
            .Where(v => v.Preuve.Regle == "VersionInfoProductVersion")
            .ToList();
        var fileVersions = versionsComparables
            .Where(v => v.Preuve.Regle == "VersionInfoFileVersion")
            .ToList();
        var versionsNom = versionsComparables
            .Where(v => v.Preuve.Source == SourcePreuveVersionnee.NomFichier)
            .ToList();

        var niveauSelectionne = structurees.Count > 0
            ? structurees
            : productVersions.Count > 0
                ? productVersions
                : fileVersions.Count > 0
                    ? fileVersions
                    : versionsNom;

        var versionsDuNiveau = niveauSelectionne.Select(v => v.Version).Distinct().ToList();
        if (versionsDuNiveau.Count > 1)
        {
            conflitVersion = true;
        }
        else if (versionsDuNiveau.Count == 1)
        {
            versionResolue = versionsDuNiveau[0];
            if (niveauSelectionne != versionsNom && versionsNom.Any(v => v.Version != versionResolue.Value))
                conflitVersion = true;
        }

        var format = ResoudreFormat(preuvesUniques, ref conflitFamille);
        var architecture = ResoudreVariante(preuvesUniques, DimensionPreuveVersionnee.Architecture, ref conflitFamille);
        var langue = ResoudreVariante(preuvesUniques, DimensionPreuveVersionnee.Langue, ref conflitFamille);
        var edition = ResoudreVariante(preuvesUniques, DimensionPreuveVersionnee.Edition, ref conflitFamille);
        var distribution = ResoudreVariante(preuvesUniques, DimensionPreuveVersionnee.Distribution, ref conflitFamille);

        var partielle = FormatsAvecArchitectureRequise.Contains(format) && architecture is null;
        if (partielle)
        {
            diagnosticsResultat.Add(new DiagnosticVersionne(
                fichierDiagnostic,
                CodeDiagnosticVersionne.VarianteNonObservee,
                SourcePreuveVersionnee.Arbitre,
                "architecture requise non observée"));
            if (confiance == NiveauConfianceVersionnee.Forte)
                confiance = NiveauConfianceVersionnee.Moyenne;
        }

        if (!conflitFamille && !conflitVersion && sourceFamille != SourcePreuveVersionnee.NomFichier &&
            AccordIndependant(preuvesUniques, libelleFamille, versionResolue))
        {
            confiance = Augmenter(confiance);
            if (partielle && confiance == NiveauConfianceVersionnee.Forte)
                confiance = NiveauConfianceVersionnee.Moyenne;
        }

        if (conflitFamille)
        {
            diagnosticsResultat.Add(new DiagnosticVersionne(
                fichierDiagnostic,
                CodeDiagnosticVersionne.ConflitDeFamille,
                SourcePreuveVersionnee.Arbitre,
                "preuves de famille ou de variante contradictoires"));
            cleFamille = null;
            libelleFamille = null;
            sourceFamille = null;
        }

        if (conflitVersion)
        {
            diagnosticsResultat.Add(new DiagnosticVersionne(
                fichierDiagnostic,
                CodeDiagnosticVersionne.ConflitDeVersion,
                SourcePreuveVersionnee.Arbitre,
                "preuves de version contradictoires"));
            versionResolue = null;
        }

        var etat = conflitFamille
            ? EtatResolutionVersionnee.ConflitDeFamille
            : conflitVersion
                ? EtatResolutionVersionnee.ConflitDeVersion
                : versionResolue is null
                    ? EtatResolutionVersionnee.VersionNonComparable
                    : EtatResolutionVersionnee.Comparable;

        return new ArtefactVersionne(
            IdentifiantsStables.NormaliserSha256(contenuSha256),
            fichiersTries,
            cleFamille,
            libelleFamille,
            sourceFamille,
            versionResolue,
            new VarianteVersionnee(format, architecture, langue, edition, distribution, partielle),
            confiance,
            etat,
            preuvesUniques,
            diagnosticsResultat
                .Distinct()
                .OrderBy(d => d.Code)
                .ThenBy(d => d.Source)
                .ThenBy(d => d.DetailNormalise, StringComparer.Ordinal)
                .ToList());
    }

    private static List<string> ValeursDistinctes(IEnumerable<PreuveVersionnee> preuves) =>
        preuves.Select(p => p.ValeurNormalisee).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();

    private static string ResoudreFormat(
        IReadOnlyList<PreuveVersionnee> preuves,
        ref bool conflit)
    {
        var natives = ValeursDistinctes(preuves.Where(p =>
            p.Dimension == DimensionPreuveVersionnee.Format &&
            p.Source is SourcePreuveVersionnee.Msi or SourcePreuveVersionnee.Appx));
        if (natives.Count > 1)
        {
            conflit = true;
            return natives[0];
        }
        if (natives.Count == 1)
            return natives[0];

        var noms = ValeursDistinctes(preuves.Where(p =>
            p.Dimension == DimensionPreuveVersionnee.Format &&
            p.Source == SourcePreuveVersionnee.NomFichier));
        if (noms.Count > 1)
            conflit = true;
        return noms.FirstOrDefault() ?? "<format-inconnu>";
    }

    private static string? ResoudreVariante(
        IReadOnlyList<PreuveVersionnee> preuves,
        DimensionPreuveVersionnee dimension,
        ref bool conflit)
    {
        var valeurs = ValeursDistinctes(preuves.Where(p => p.Dimension == dimension));
        if (valeurs.Count > 1)
            conflit = true;
        return valeurs.FirstOrDefault();
    }

    private static bool AccordIndependant(
        IReadOnlyList<PreuveVersionnee> preuves,
        string? libelleFamille,
        VersionComparable? version)
    {
        if (libelleFamille is null || version is null)
            return false;

        var famille = NormalisationVersionnee.Texte(libelleFamille);
        var nomConcordant = preuves.Any(p =>
            p.Source == SourcePreuveVersionnee.NomFichier &&
            p.Dimension == DimensionPreuveVersionnee.LibelleFamille &&
            p.ValeurNormalisee == famille);
        var versionConcordante = preuves.Any(p =>
            p.Source == SourcePreuveVersionnee.NomFichier &&
            p.Dimension == DimensionPreuveVersionnee.Version &&
            VersionComparable.TryLire(p.ValeurNormalisee, false, out var candidate) &&
            candidate == version.Value);
        return nomConcordant && versionConcordante;
    }

    private static NiveauConfianceVersionnee Augmenter(NiveauConfianceVersionnee niveau) => niveau switch
    {
        NiveauConfianceVersionnee.Faible => NiveauConfianceVersionnee.Moyenne,
        NiveauConfianceVersionnee.Moyenne => NiveauConfianceVersionnee.Forte,
        _ => NiveauConfianceVersionnee.Forte,
    };

    private static string CleComposite(string prefixe, params string[] champs)
    {
        var charge = string.Join("\n", champs.Select(c => $"{Encoding.UTF8.GetByteCount(c)}:{c}"));
        var empreinte = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(charge))).ToLowerInvariant();
        return $"{prefixe}:{empreinte}";
    }
}
