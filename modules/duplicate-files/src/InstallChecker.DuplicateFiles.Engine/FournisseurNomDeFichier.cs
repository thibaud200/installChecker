using System.Text.RegularExpressions;

namespace InstallChecker.DuplicateFiles;

public sealed class FournisseurNomDeFichier : IFournisseurDePreuves
{
    public const string Version = "filename/v1";

    private static readonly Regex FormeNomVersionne = new(
        @"^(?<famille>.+?)[\s._-]+(?<version>\d{4}-\d{2}-\d{2}|v?\d+(?:\.\d+){0,3})(?<suffixe>(?:[\s._-]+[\p{L}\p{Nd}]+)*)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex Separateurs = new(
        @"[\s._-]+",
        RegexOptions.CultureInvariant);

    private static readonly Regex SuffixePreliminaire = new(
        @"^(?:(?:alpha|beta|preview|rc)\d*|rev[\p{L}\p{Nd}]+|final\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> FamillesGeneriques = new(
        ["SETUP", "INSTALL", "INSTALLER", "UPDATE", "PACKAGE"],
        StringComparer.Ordinal);

    public ResultatFournisseur Extraire(FichierObserve fichier)
    {
        ArgumentNullException.ThrowIfNull(fichier);

        var preuves = new List<PreuveVersionnee>();
        var diagnostics = new List<DiagnosticVersionne>();
        var format = NormalisationVersionnee.Format(fichier.Chemin);
        AjouterPreuve(
            preuves,
            fichier,
            DimensionPreuveVersionnee.Format,
            format,
            format,
            "NomFormat");

        var nom = Path.GetFileName(fichier.Chemin);
        var radical = format == "<sans-extension>" ? nom : nom[..^format.Length];
        var correspondance = FormeNomVersionne.Match(radical);
        if (!correspondance.Success)
            return new ResultatFournisseur(preuves, diagnostics);

        var suffixes = Separateurs
            .Split(correspondance.Groups["suffixe"].Value)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        var variantes = new List<(DimensionPreuveVersionnee Dimension, string Brute, string Normalisee, string Regle)>();
        var estPreliminaire = false;
        foreach (var suffixe in suffixes)
        {
            if (SuffixePreliminaire.IsMatch(suffixe))
            {
                estPreliminaire = true;
                continue;
            }

            var architecture = ArchitectureConnue(suffixe);
            if (architecture is not null)
            {
                variantes.Add((
                    DimensionPreuveVersionnee.Architecture,
                    suffixe,
                    architecture,
                    "NomArchitecture"));
                continue;
            }

            var distribution = DistributionConnue(suffixe);
            if (distribution is not null)
            {
                variantes.Add((
                    DimensionPreuveVersionnee.Distribution,
                    suffixe,
                    distribution,
                    "NomDistribution"));
                continue;
            }

            return new ResultatFournisseur(preuves, diagnostics);
        }

        var familleBrute = correspondance.Groups["famille"].Value.Trim(' ', '.', '_', '-');
        if (!string.IsNullOrWhiteSpace(familleBrute))
        {
            var familleNormalisee = NormalisationVersionnee.Texte(familleBrute);
            if (!FamillesGeneriques.Contains(familleNormalisee))
            {
                AjouterPreuve(
                    preuves,
                    fichier,
                    DimensionPreuveVersionnee.LibelleFamille,
                    familleBrute,
                    familleNormalisee,
                    "NomRadical");
            }
        }

        var versionBrute = correspondance.Groups["version"].Value;
        if (estPreliminaire)
        {
            var complete = string.Join('-', [versionBrute, .. suffixes]);
            AjouterPreuve(
                preuves,
                fichier,
                DimensionPreuveVersionnee.Version,
                complete,
                NormalisationVersionnee.Texte(complete),
                "NomVersionNonComparable");
            diagnostics.Add(new DiagnosticVersionne(
                fichier.FichierId,
                CodeDiagnosticVersionne.VersionNonComparable,
                SourcePreuveVersionnee.NomFichier,
                "suffixe de version non comparable en F1"));
        }
        else if (VersionComparable.TryLire(versionBrute, autoriserPrefixeV: true, out var version))
        {
            AjouterPreuve(
                preuves,
                fichier,
                DimensionPreuveVersionnee.Version,
                versionBrute,
                version.Canonique,
                "NomVersion");
        }
        else
        {
            AjouterPreuve(
                preuves,
                fichier,
                DimensionPreuveVersionnee.Version,
                versionBrute,
                NormalisationVersionnee.Texte(versionBrute),
                "NomVersionNonComparable");
            diagnostics.Add(new DiagnosticVersionne(
                fichier.FichierId,
                CodeDiagnosticVersionne.VersionNonComparable,
                SourcePreuveVersionnee.NomFichier,
                "version du nom non comparable en F1"));
        }

        foreach (var variante in variantes)
        {
            AjouterPreuve(
                preuves,
                fichier,
                variante.Dimension,
                variante.Brute,
                variante.Normalisee,
                variante.Regle);
        }

        return new ResultatFournisseur(preuves, diagnostics);
    }

    private static string? ArchitectureConnue(string valeur)
    {
        var candidate = valeur.ToUpperInvariant();
        return candidate is "X86" or "WIN32" or "X64" or "AMD64" or "WIN64" or
            "ARM" or "ARM64" or "AARCH64"
                ? NormalisationVersionnee.Architecture(valeur)
                : null;
    }

    private static string? DistributionConnue(string valeur) => valeur.ToUpperInvariant() switch
    {
        "PORTABLE" => "portable",
        "SETUP" or "INSTALLER" or "INSTALL" => "installable",
        _ => null,
    };

    private static void AjouterPreuve(
        ICollection<PreuveVersionnee> preuves,
        FichierObserve fichier,
        DimensionPreuveVersionnee dimension,
        string brute,
        string normalisee,
        string regle) =>
        preuves.Add(new PreuveVersionnee(
            fichier.FichierId,
            dimension,
            brute,
            normalisee,
            SourcePreuveVersionnee.NomFichier,
            ForcePreuveVersionnee.Faible,
            regle,
            Version));
}
