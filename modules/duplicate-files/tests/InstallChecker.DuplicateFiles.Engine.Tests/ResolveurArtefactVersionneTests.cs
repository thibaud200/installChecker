using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class ResolveurArtefactVersionneTests
{
    private const string Hash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact]
    public void Une_cle_Msi_est_prioritaire_et_larchitecture_absente_reduit_la_confiance()
    {
        var fichier = Fichier(@"C:\corpus\outil-2.0.msi", new Dictionary<Attribut, ValeurObservee>
        {
            [new("msi_properties", "upgrade_code")] = new ValeurObservee.Texte("{12345678-1234-1234-1234-1234567890AB}"),
            [new("msi_properties", "product_name")] = new ValeurObservee.Texte("Outil Exemple"),
            [new("msi_properties", "product_version")] = new ValeurObservee.Texte("2.0"),
            [new("msi_properties", "product_language")] = new ValeurObservee.Texte("1036"),
        });

        var artefact = Resoudre([fichier], new FournisseurNomDeFichier(), new FournisseurMsi());

        Assert.Equal("12345678-1234-1234-1234-1234567890ab", artefact.CleFamille);
        Assert.Equal("Outil Exemple", artefact.LibelleFamille);
        Assert.Equal(SourcePreuveVersionnee.Msi, artefact.SourceFamille);
        Assert.Equal("2", artefact.Version?.Canonique);
        Assert.Equal(".msi", artefact.Variante.Format);
        Assert.Equal("1036", artefact.Variante.Langue);
        Assert.True(artefact.Variante.Partielle);
        Assert.Equal(NiveauConfianceVersionnee.Moyenne, artefact.Confiance);
        Assert.Contains(artefact.Diagnostics, d => d.Code == CodeDiagnosticVersionne.VarianteNonObservee);
    }

    [Fact]
    public void ProductName_et_CompanyName_forment_la_cle_de_repli_et_le_nom_concordant_la_renforce()
    {
        var fichier = Fichier(@"C:\corpus\Outil Exemple-2.0-x64.exe", new Dictionary<Attribut, ValeurObservee>
        {
            [new("version_info", "product_name")] = new ValeurObservee.Texte("Outil Exemple"),
            [new("version_info", "company_name")] = new ValeurObservee.Texte("Éditeur Exemple"),
            [new("version_info", "product_version")] = new ValeurObservee.Texte("2.0.0"),
            [new("pe_info", "machine")] = new ValeurObservee.Texte("AMD64"),
        });

        var artefact = Resoudre(
            [fichier],
            new FournisseurNomDeFichier(),
            new FournisseurVersionInfo(),
            new FournisseurPe());

        Assert.StartsWith("versioninfo-family:", artefact.CleFamille);
        Assert.Equal("Outil Exemple", artefact.LibelleFamille);
        Assert.Equal(SourcePreuveVersionnee.VersionInfo, artefact.SourceFamille);
        Assert.Equal("2", artefact.Version?.Canonique);
        Assert.Equal("x64", artefact.Variante.Architecture);
        Assert.False(artefact.Variante.Partielle);
        Assert.Equal(NiveauConfianceVersionnee.Forte, artefact.Confiance);
        Assert.Equal(EtatResolutionVersionnee.Comparable, artefact.Etat);
    }

    [Fact]
    public void Un_nom_seul_produit_une_famille_faible()
    {
        var fichier = Fichier(@"C:\corpus\archive-1.2.zip", new Dictionary<Attribut, ValeurObservee>());

        var artefact = Resoudre([fichier], new FournisseurNomDeFichier());

        Assert.Equal("filename-family:ARCHIVE", artefact.CleFamille);
        Assert.Equal(NiveauConfianceVersionnee.Faible, artefact.Confiance);
        Assert.Equal("1.2", artefact.Version?.Canonique);
        Assert.False(artefact.Variante.Partielle);
    }

    [Fact]
    public void Un_desaccord_entre_nom_et_ProductVersion_bloque_la_version()
    {
        var fichier = Fichier(@"C:\corpus\outil-2.0-x64.exe", new Dictionary<Attribut, ValeurObservee>
        {
            [new("version_info", "product_name")] = new ValeurObservee.Texte("outil"),
            [new("version_info", "company_name")] = new ValeurObservee.Texte("éditeur"),
            [new("version_info", "product_version")] = new ValeurObservee.Texte("1.9"),
            [new("pe_info", "machine")] = new ValeurObservee.Texte("x64"),
        });

        var artefact = Resoudre(
            [fichier],
            new FournisseurNomDeFichier(),
            new FournisseurVersionInfo(),
            new FournisseurPe());

        Assert.Equal(EtatResolutionVersionnee.ConflitDeVersion, artefact.Etat);
        Assert.Null(artefact.Version);
        Assert.Contains(artefact.Diagnostics, d =>
            d.Code == CodeDiagnosticVersionne.ConflitDeVersion && d.Source == SourcePreuveVersionnee.Arbitre);
    }

    [Fact]
    public void ProductVersion_ne_conflite_pas_avec_FileVersion()
    {
        var fichier = Fichier(@"C:\corpus\outil.exe", new Dictionary<Attribut, ValeurObservee>());
        var preuves = new[]
        {
            Preuve(fichier, DimensionPreuveVersionnee.LibelleFamille, "outil", "OUTIL", SourcePreuveVersionnee.VersionInfo, ForcePreuveVersionnee.Moyenne, "VersionInfoProductName"),
            Preuve(fichier, DimensionPreuveVersionnee.Editeur, "éditeur", "ÉDITEUR", SourcePreuveVersionnee.VersionInfo, ForcePreuveVersionnee.Moyenne, "VersionInfoCompanyName"),
            Preuve(fichier, DimensionPreuveVersionnee.Version, "2.0", "2", SourcePreuveVersionnee.VersionInfo, ForcePreuveVersionnee.Moyenne, "VersionInfoProductVersion"),
            Preuve(fichier, DimensionPreuveVersionnee.Version, "2.0.1534.0", "2.0.1534", SourcePreuveVersionnee.VersionInfo, ForcePreuveVersionnee.Moyenne, "VersionInfoFileVersion"),
            Preuve(fichier, DimensionPreuveVersionnee.Format, ".exe", ".exe", SourcePreuveVersionnee.NomFichier, ForcePreuveVersionnee.Faible, "NomFormat"),
            Preuve(fichier, DimensionPreuveVersionnee.Architecture, "x64", "x64", SourcePreuveVersionnee.Pe, ForcePreuveVersionnee.Forte, "PeMachine"),
        };

        var artefact = ResolveurArtefactVersionne.Resoudre(Hash, [fichier], preuves, []);

        Assert.Equal(EtatResolutionVersionnee.Comparable, artefact.Etat);
        Assert.Equal("2", artefact.Version?.Canonique);
    }

    [Fact]
    public void Deux_cles_natives_incompatibles_bloquent_la_famille()
    {
        var fichier = Fichier(@"C:\corpus\outil.bin", new Dictionary<Attribut, ValeurObservee>());
        var preuves = new[]
        {
            Preuve(fichier, DimensionPreuveVersionnee.CleFamille, "A", "A", SourcePreuveVersionnee.Msi, ForcePreuveVersionnee.Forte, "MsiUpgradeCode"),
            Preuve(fichier, DimensionPreuveVersionnee.CleFamille, "B", "B", SourcePreuveVersionnee.Appx, ForcePreuveVersionnee.Forte, "AppxNamePublisher"),
        };

        var artefact = ResolveurArtefactVersionne.Resoudre(Hash, [fichier], preuves, []);

        Assert.Equal(EtatResolutionVersionnee.ConflitDeFamille, artefact.Etat);
        Assert.Null(artefact.CleFamille);
    }

    [Fact]
    public void Deux_noms_contradictoires_du_meme_contenu_ne_votent_pas()
    {
        var premier = Fichier(@"C:\corpus\outil-1.0.zip", new Dictionary<Attribut, ValeurObservee>(), 1);
        var second = Fichier(@"D:\archives\produit-2.0.zip", new Dictionary<Attribut, ValeurObservee>(), 2);
        var fournisseur = new FournisseurNomDeFichier();
        var r1 = fournisseur.Extraire(premier);
        var r2 = fournisseur.Extraire(second);

        var artefact = ResolveurArtefactVersionne.Resoudre(
            Hash,
            [premier, second],
            [.. r1.Preuves, .. r2.Preuves],
            [.. r1.Diagnostics, .. r2.Diagnostics]);

        Assert.Equal(EtatResolutionVersionnee.ConflitDeFamille, artefact.Etat);
    }

    [Fact]
    public void Deux_architectures_fortes_sur_un_meme_contenu_sont_un_conflit()
    {
        var fichier = Fichier(@"C:\corpus\outil.exe", new Dictionary<Attribut, ValeurObservee>());
        var preuves = new[]
        {
            Preuve(fichier, DimensionPreuveVersionnee.CleFamille, "A", "A", SourcePreuveVersionnee.Appx, ForcePreuveVersionnee.Forte, "AppxNamePublisher"),
            Preuve(fichier, DimensionPreuveVersionnee.Version, "1", "1", SourcePreuveVersionnee.Appx, ForcePreuveVersionnee.Forte, "AppxVersion"),
            Preuve(fichier, DimensionPreuveVersionnee.Format, ".msix", ".msix", SourcePreuveVersionnee.Appx, ForcePreuveVersionnee.Forte, "AppxFormat"),
            Preuve(fichier, DimensionPreuveVersionnee.Architecture, "x64", "x64", SourcePreuveVersionnee.Appx, ForcePreuveVersionnee.Forte, "AppxProcessorArchitecture"),
            Preuve(fichier, DimensionPreuveVersionnee.Architecture, "arm64", "arm64", SourcePreuveVersionnee.Pe, ForcePreuveVersionnee.Forte, "PeMachine"),
        };

        var artefact = ResolveurArtefactVersionne.Resoudre(Hash, [fichier], preuves, []);

        Assert.Equal(EtatResolutionVersionnee.ConflitDeFamille, artefact.Etat);
    }

    private static ArtefactVersionne Resoudre(
        IReadOnlyList<FichierObserve> fichiers,
        params IFournisseurDePreuves[] fournisseurs)
    {
        var preuves = new List<PreuveVersionnee>();
        var diagnostics = new List<DiagnosticVersionne>();
        foreach (var fichier in fichiers)
        foreach (var fournisseur in fournisseurs)
        {
            var resultat = fournisseur.Extraire(fichier);
            preuves.AddRange(resultat.Preuves);
            diagnostics.AddRange(resultat.Diagnostics);
        }

        return ResolveurArtefactVersionne.Resoudre(Hash, fichiers, preuves, diagnostics);
    }

    private static FichierObserve Fichier(
        string chemin,
        IReadOnlyDictionary<Attribut, ValeurObservee> attributs,
        long acteId = 1) =>
        new(
            acteId,
            IdentifiantsStables.PourFichier(Hash, chemin),
            chemin,
            100,
            Hash,
            attributs);

    private static PreuveVersionnee Preuve(
        FichierObserve fichier,
        DimensionPreuveVersionnee dimension,
        string brute,
        string normalisee,
        SourcePreuveVersionnee source,
        ForcePreuveVersionnee force,
        string regle) =>
        new(fichier.FichierId, dimension, brute, normalisee, source, force, regle, "test/v1");
}
