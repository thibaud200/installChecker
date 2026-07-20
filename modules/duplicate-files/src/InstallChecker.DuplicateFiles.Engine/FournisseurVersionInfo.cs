namespace InstallChecker.DuplicateFiles;

public sealed class FournisseurVersionInfo : IFournisseurDePreuves
{
    public const string Version = "version-info/v1";

    public ResultatFournisseur Extraire(FichierObserve fichier)
    {
        ArgumentNullException.ThrowIfNull(fichier);

        var nomProduit = LectureAttributs.Texte(fichier, "version_info", "product_name");
        var editeur = LectureAttributs.Texte(fichier, "version_info", "company_name");
        var versionProduit = LectureAttributs.Texte(fichier, "version_info", "product_version");
        var versionFichier = LectureAttributs.Texte(fichier, "version_info", "file_version");

        if (nomProduit is null && editeur is null && versionProduit is null && versionFichier is null)
            return ResultatFournisseur.Vide;

        var preuves = new List<PreuveVersionnee>();
        var diagnostics = new List<DiagnosticVersionne>();

        if (nomProduit is not null)
            AjouterTexte(preuves, fichier, DimensionPreuveVersionnee.LibelleFamille, nomProduit, "VersionInfoProductName");
        if (editeur is not null)
            AjouterTexte(preuves, fichier, DimensionPreuveVersionnee.Editeur, editeur, "VersionInfoCompanyName");

        if (versionProduit is not null)
            AjouterVersion(preuves, diagnostics, fichier, versionProduit, "VersionInfoProductVersion");
        else if (versionFichier is not null)
            AjouterVersion(preuves, diagnostics, fichier, versionFichier, "VersionInfoFileVersion");

        return new ResultatFournisseur(preuves, diagnostics);
    }

    private static void AjouterTexte(
        ICollection<PreuveVersionnee> preuves,
        FichierObserve fichier,
        DimensionPreuveVersionnee dimension,
        string brute,
        string regle) =>
        preuves.Add(new PreuveVersionnee(
            fichier.FichierId,
            dimension,
            brute,
            NormalisationVersionnee.Texte(brute),
            SourcePreuveVersionnee.VersionInfo,
            ForcePreuveVersionnee.Moyenne,
            regle,
            Version));

    private static void AjouterVersion(
        ICollection<PreuveVersionnee> preuves,
        ICollection<DiagnosticVersionne> diagnostics,
        FichierObserve fichier,
        string brute,
        string regle)
    {
        var comparable = VersionComparable.TryLire(brute, autoriserPrefixeV: false, out var version);
        preuves.Add(new PreuveVersionnee(
            fichier.FichierId,
            DimensionPreuveVersionnee.Version,
            brute,
            comparable ? version.Canonique : NormalisationVersionnee.Texte(brute),
            SourcePreuveVersionnee.VersionInfo,
            ForcePreuveVersionnee.Moyenne,
            regle,
            Version));

        if (!comparable)
        {
            diagnostics.Add(new DiagnosticVersionne(
                fichier.FichierId,
                CodeDiagnosticVersionne.VersionNonComparable,
                SourcePreuveVersionnee.VersionInfo,
                $"{regle} non comparable en F1"));
        }
    }
}
