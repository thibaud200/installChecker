using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class FournisseurNomDeFichierTests
{
    private const string Hash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private readonly FournisseurNomDeFichier _fournisseur = new();

    [Theory]
    [InlineData(@"C:\corpus\outil-1.2.zip", "OUTIL", "1.2", ".zip")]
    [InlineData(@"C:\corpus\rapport-2026-07-19.pdf", "RAPPORT", "2026-07-19", ".pdf")]
    [InlineData(@"C:\corpus\archive-1.2.tar.gz", "ARCHIVE", "1.2", ".tar.gz")]
    public void Le_nom_fournit_une_famille_une_version_et_un_format(
        string chemin,
        string famille,
        string version,
        string format)
    {
        var resultat = _fournisseur.Extraire(Fichier(chemin));

        AssertPreuve(resultat, DimensionPreuveVersionnee.LibelleFamille, famille);
        AssertPreuve(resultat, DimensionPreuveVersionnee.Version, version);
        AssertPreuve(resultat, DimensionPreuveVersionnee.Format, format);
        Assert.All(resultat.Preuves, p => Assert.Equal(ForcePreuveVersionnee.Faible, p.Force));
        Assert.All(resultat.Preuves, p => Assert.Equal(SourcePreuveVersionnee.NomFichier, p.Source));
        Assert.All(resultat.Preuves, p => Assert.Equal("filename/v1", p.VersionFournisseur));
        Assert.Empty(resultat.Diagnostics);
    }

    [Fact]
    public void Les_variantes_reconnues_apres_la_version_sont_extraites()
    {
        var x64 = _fournisseur.Extraire(Fichier(@"C:\corpus\outil_v1.3_AMD64.zip"));
        var portable = _fournisseur.Extraire(Fichier(@"C:\corpus\outil-2.0-portable.zip"));
        var installable = _fournisseur.Extraire(Fichier(@"C:\corpus\outil-2.1-installer.zip"));

        AssertPreuve(x64, DimensionPreuveVersionnee.Architecture, "x64");
        AssertPreuve(portable, DimensionPreuveVersionnee.Distribution, "portable");
        AssertPreuve(installable, DimensionPreuveVersionnee.Distribution, "installable");
    }

    [Fact]
    public void Une_edition_placee_avant_la_version_reste_dans_la_famille()
    {
        var pro = _fournisseur.Extraire(Fichier(@"C:\corpus\outil-pro-1.2.zip"));
        var home = _fournisseur.Extraire(Fichier(@"C:\corpus\outil-home-1.3.zip"));

        AssertPreuve(pro, DimensionPreuveVersionnee.LibelleFamille, "OUTIL-PRO");
        AssertPreuve(home, DimensionPreuveVersionnee.LibelleFamille, "OUTIL-HOME");
    }

    [Theory]
    [InlineData(@"C:\corpus\setup-1.2.exe")]
    [InlineData(@"C:\corpus\update-2026-07-19.zip")]
    public void Un_radical_generique_ne_devient_pas_une_famille(string chemin)
    {
        var resultat = _fournisseur.Extraire(Fichier(chemin));

        Assert.DoesNotContain(resultat.Preuves, p => p.Dimension == DimensionPreuveVersionnee.LibelleFamille);
        Assert.Contains(resultat.Preuves, p => p.Dimension == DimensionPreuveVersionnee.Version);
    }

    [Theory]
    [InlineData(@"C:\corpus\outil-2.0-beta.zip")]
    [InlineData(@"C:\corpus\outil-2.0-rc1.zip")]
    [InlineData(@"C:\corpus\outil-2.0-revA.zip")]
    public void Un_suffixe_preliminaire_est_conserve_mais_non_comparable(string chemin)
    {
        var resultat = _fournisseur.Extraire(Fichier(chemin));

        Assert.Contains(resultat.Preuves, p => p.Dimension == DimensionPreuveVersionnee.Version);
        Assert.Contains(resultat.Diagnostics, d => d.Code == CodeDiagnosticVersionne.VersionNonComparable);
    }

    [Fact]
    public void Un_suffixe_inconnu_interdit_lextraction_de_la_version()
    {
        var resultat = _fournisseur.Extraire(Fichier(@"C:\corpus\outil-2.0-inconnu.zip"));

        Assert.DoesNotContain(resultat.Preuves, p => p.Dimension == DimensionPreuveVersionnee.Version);
        Assert.DoesNotContain(resultat.Preuves, p => p.Dimension == DimensionPreuveVersionnee.LibelleFamille);
    }

    [Fact]
    public void Un_nom_sans_version_ne_produit_que_le_format()
    {
        var resultat = _fournisseur.Extraire(Fichier(@"C:\corpus\readme.pdf"));

        var preuve = Assert.Single(resultat.Preuves);
        Assert.Equal(DimensionPreuveVersionnee.Format, preuve.Dimension);
        Assert.Equal(".pdf", preuve.ValeurNormalisee);
        Assert.Empty(resultat.Diagnostics);
    }

    private static FichierObserve Fichier(string chemin) => new(
        1,
        IdentifiantsStables.PourFichier(Hash, chemin),
        chemin,
        100,
        Hash,
        new Dictionary<Attribut, ValeurObservee>());

    private static void AssertPreuve(
        ResultatFournisseur resultat,
        DimensionPreuveVersionnee dimension,
        string valeur) =>
        Assert.Contains(
            resultat.Preuves,
            p => p.Dimension == dimension && p.ValeurNormalisee == valeur);
}
