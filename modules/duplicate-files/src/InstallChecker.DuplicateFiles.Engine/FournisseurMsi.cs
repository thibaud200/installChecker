namespace InstallChecker.DuplicateFiles;

public sealed class FournisseurMsi : IFournisseurDePreuves
{
    public const string Version = "msi/v1";

    public ResultatFournisseur Extraire(FichierObserve fichier)
    {
        ArgumentNullException.ThrowIfNull(fichier);

        var upgradeCode = LectureAttributs.Texte(fichier, "msi_properties", "upgrade_code");
        var productCode = LectureAttributs.Texte(fichier, "msi_properties", "product_code");
        var productName = LectureAttributs.Texte(fichier, "msi_properties", "product_name");
        var productVersion = LectureAttributs.Texte(fichier, "msi_properties", "product_version");
        var manufacturer = LectureAttributs.Texte(fichier, "msi_properties", "manufacturer");
        var productLanguage = LectureAttributs.Texte(fichier, "msi_properties", "product_language");

        if (upgradeCode is null && productCode is null && productName is null &&
            productVersion is null && manufacturer is null && productLanguage is null)
        {
            return ResultatFournisseur.Vide;
        }

        var preuves = new List<PreuveVersionnee>();
        var diagnostics = new List<DiagnosticVersionne>();
        Ajouter(preuves, fichier, DimensionPreuveVersionnee.Format, ".msi", ".msi", "MsiFormat");

        if (upgradeCode is not null)
        {
            if (Guid.TryParse(upgradeCode, out var guid))
                Ajouter(preuves, fichier, DimensionPreuveVersionnee.CleFamille, upgradeCode, guid.ToString("D"), "MsiUpgradeCode");
            else
                AjouterDiagnostic(diagnostics, fichier, CodeDiagnosticVersionne.AttributInvalide, "MsiUpgradeCode invalide");
        }

        if (productCode is not null)
        {
            var normalise = Guid.TryParse(productCode, out var guid)
                ? guid.ToString("D").ToUpperInvariant()
                : NormalisationVersionnee.Texte(productCode);
            Ajouter(preuves, fichier, DimensionPreuveVersionnee.IdentifiantLivraison, productCode, normalise, "MsiProductCode");
        }

        if (productName is not null)
            AjouterTexte(preuves, fichier, DimensionPreuveVersionnee.LibelleFamille, productName, "MsiProductName");
        if (manufacturer is not null)
            AjouterTexte(preuves, fichier, DimensionPreuveVersionnee.Editeur, manufacturer, "MsiManufacturer");
        if (productLanguage is not null)
            AjouterTexte(preuves, fichier, DimensionPreuveVersionnee.Langue, productLanguage, "MsiProductLanguage");
        if (productVersion is not null)
            AjouterVersion(preuves, diagnostics, fichier, productVersion);

        return new ResultatFournisseur(preuves, diagnostics);
    }

    private static void AjouterTexte(
        ICollection<PreuveVersionnee> preuves,
        FichierObserve fichier,
        DimensionPreuveVersionnee dimension,
        string brute,
        string regle) =>
        Ajouter(preuves, fichier, dimension, brute, NormalisationVersionnee.Texte(brute), regle);

    private static void AjouterVersion(
        ICollection<PreuveVersionnee> preuves,
        ICollection<DiagnosticVersionne> diagnostics,
        FichierObserve fichier,
        string brute)
    {
        var comparable = VersionComparable.TryLire(brute, autoriserPrefixeV: false, out var version);
        Ajouter(
            preuves,
            fichier,
            DimensionPreuveVersionnee.Version,
            brute,
            comparable ? version.Canonique : NormalisationVersionnee.Texte(brute),
            "MsiProductVersion");
        if (!comparable)
            AjouterDiagnostic(diagnostics, fichier, CodeDiagnosticVersionne.VersionNonComparable, "MsiProductVersion non comparable en F1");
    }

    private static void Ajouter(
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
            SourcePreuveVersionnee.Msi,
            ForcePreuveVersionnee.Forte,
            regle,
            Version));

    private static void AjouterDiagnostic(
        ICollection<DiagnosticVersionne> diagnostics,
        FichierObserve fichier,
        CodeDiagnosticVersionne code,
        string detail) =>
        diagnostics.Add(new DiagnosticVersionne(
            fichier.FichierId,
            code,
            SourcePreuveVersionnee.Msi,
            detail));
}
