using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class FournisseursMetadonneesTests
{
    private const string Hash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact]
    public void VersionInfo_extrait_produit_editeur_et_version_produit()
    {
        var fichier = Fichier(new Dictionary<Attribut, ValeurObservee>
        {
            [new("version_info", "product_name")] = new ValeurObservee.Texte("Outil Exemple"),
            [new("version_info", "company_name")] = new ValeurObservee.Texte("Éditeur Exemple"),
            [new("version_info", "product_version")] = new ValeurObservee.Texte("2.1.0"),
            [new("version_info", "file_version")] = new ValeurObservee.Texte("2.1.1534.0"),
        });

        var resultat = new FournisseurVersionInfo().Extraire(fichier);

        AssertPreuve(resultat, DimensionPreuveVersionnee.LibelleFamille, "OUTIL EXEMPLE", "VersionInfoProductName");
        AssertPreuve(resultat, DimensionPreuveVersionnee.Editeur, "ÉDITEUR EXEMPLE", "VersionInfoCompanyName");
        AssertPreuve(resultat, DimensionPreuveVersionnee.Version, "2.1", "VersionInfoProductVersion");
        Assert.DoesNotContain(resultat.Preuves, p => p.Regle == "VersionInfoFileVersion");
        Assert.All(resultat.Preuves, p => Assert.Equal(ForcePreuveVersionnee.Moyenne, p.Force));
        Assert.Empty(resultat.Diagnostics);
    }

    [Fact]
    public void FileVersion_est_un_repli_uniquement_si_ProductVersion_est_absente()
    {
        var fichier = Fichier(new Dictionary<Attribut, ValeurObservee>
        {
            [new("version_info", "file_version")] = new ValeurObservee.Texte("3.4.5.0"),
        });

        var resultat = new FournisseurVersionInfo().Extraire(fichier);

        AssertPreuve(resultat, DimensionPreuveVersionnee.Version, "3.4.5", "VersionInfoFileVersion");
    }

    [Fact]
    public void Une_version_structuree_invalide_est_expliquee_sans_repli()
    {
        var fichier = Fichier(new Dictionary<Attribut, ValeurObservee>
        {
            [new("version_info", "product_version")] = new ValeurObservee.Texte("2.0 beta"),
            [new("version_info", "file_version")] = new ValeurObservee.Texte("2.0.42.0"),
        });

        var resultat = new FournisseurVersionInfo().Extraire(fichier);

        Assert.Contains(resultat.Preuves, p => p.Regle == "VersionInfoProductVersion");
        Assert.DoesNotContain(resultat.Preuves, p => p.Regle == "VersionInfoFileVersion");
        Assert.Contains(resultat.Diagnostics, d => d.Code == CodeDiagnosticVersionne.VersionNonComparable);
    }

    [Theory]
    [InlineData("AMD64", "x64")]
    [InlineData("I386", "x86")]
    [InlineData("014c", "x86")]
    [InlineData("8664", "x64")]
    [InlineData("aa64", "arm64")]
    public void Pe_extrait_larchitecture(string brute, string normalisee)
    {
        var fichier = Fichier(new Dictionary<Attribut, ValeurObservee>
        {
            [new("pe_info", "machine")] = new ValeurObservee.Texte(brute),
        });

        var resultat = new FournisseurPe().Extraire(fichier);

        AssertPreuve(resultat, DimensionPreuveVersionnee.Architecture, normalisee, "PeMachine");
        Assert.All(resultat.Preuves, p => Assert.Equal(ForcePreuveVersionnee.Forte, p.Force));
    }

    [Fact]
    public void Authenticode_extrait_un_editeur_signe_fort()
    {
        var fichier = Fichier(new Dictionary<Attribut, ValeurObservee>
        {
            [new("authenticode", "subject")] = new ValeurObservee.Texte("CN=Éditeur Exemple"),
        });

        var resultat = new FournisseurAuthenticode().Extraire(fichier);

        AssertPreuve(resultat, DimensionPreuveVersionnee.Editeur, "CN=ÉDITEUR EXEMPLE", "AuthenticodeSubject");
        Assert.Equal(ForcePreuveVersionnee.Forte, Assert.Single(resultat.Preuves).Force);
    }

    [Fact]
    public void Des_attributs_absents_rendent_les_fournisseurs_non_applicables()
    {
        var fichier = Fichier(new Dictionary<Attribut, ValeurObservee>());

        Assert.Equal(ResultatFournisseur.Vide, new FournisseurVersionInfo().Extraire(fichier));
        Assert.Equal(ResultatFournisseur.Vide, new FournisseurPe().Extraire(fichier));
        Assert.Equal(ResultatFournisseur.Vide, new FournisseurAuthenticode().Extraire(fichier));
    }

    private static FichierObserve Fichier(IReadOnlyDictionary<Attribut, ValeurObservee> attributs)
    {
        const string chemin = @"C:\corpus\outil.exe";
        return new FichierObserve(
            1,
            IdentifiantsStables.PourFichier(Hash, chemin),
            chemin,
            100,
            Hash,
            attributs);
    }

    private static void AssertPreuve(
        ResultatFournisseur resultat,
        DimensionPreuveVersionnee dimension,
        string valeur,
        string regle) =>
        Assert.Contains(
            resultat.Preuves,
            p => p.Dimension == dimension && p.ValeurNormalisee == valeur && p.Regle == regle);
}
