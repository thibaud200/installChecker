using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class FournisseursPaquetsTests
{
    private const string Hash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact]
    public void Msi_extrait_sa_cle_native_sa_version_son_editeur_et_sa_langue()
    {
        var fichier = Fichier(@"C:\corpus\outil.msi", new Dictionary<Attribut, ValeurObservee>
        {
            [new("msi_properties", "upgrade_code")] = new ValeurObservee.Texte("{12345678-1234-1234-1234-1234567890AB}"),
            [new("msi_properties", "product_code")] = new ValeurObservee.Texte("{AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE}"),
            [new("msi_properties", "product_name")] = new ValeurObservee.Texte("Outil Exemple"),
            [new("msi_properties", "product_version")] = new ValeurObservee.Texte("2.1.0"),
            [new("msi_properties", "manufacturer")] = new ValeurObservee.Texte("Éditeur Exemple"),
            [new("msi_properties", "product_language")] = new ValeurObservee.Texte("1036"),
        });

        var resultat = new FournisseurMsi().Extraire(fichier);

        AssertPreuve(resultat, DimensionPreuveVersionnee.CleFamille, "12345678-1234-1234-1234-1234567890ab");
        AssertPreuve(resultat, DimensionPreuveVersionnee.IdentifiantLivraison, "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");
        AssertPreuve(resultat, DimensionPreuveVersionnee.LibelleFamille, "OUTIL EXEMPLE");
        AssertPreuve(resultat, DimensionPreuveVersionnee.Version, "2.1");
        AssertPreuve(resultat, DimensionPreuveVersionnee.Editeur, "ÉDITEUR EXEMPLE");
        AssertPreuve(resultat, DimensionPreuveVersionnee.Langue, "1036");
        AssertPreuve(resultat, DimensionPreuveVersionnee.Format, ".msi");
        Assert.DoesNotContain(resultat.Preuves, p => p.Dimension == DimensionPreuveVersionnee.Architecture);
        Assert.All(resultat.Preuves, p => Assert.Equal(ForcePreuveVersionnee.Forte, p.Force));
        Assert.Empty(resultat.Diagnostics);
    }

    [Fact]
    public void Un_UpgradeCode_invalide_est_diagnostique_sans_cle_native()
    {
        var fichier = Fichier(@"C:\corpus\outil.msi", new Dictionary<Attribut, ValeurObservee>
        {
            [new("msi_properties", "upgrade_code")] = new ValeurObservee.Texte("pas-un-guid"),
            [new("msi_properties", "product_name")] = new ValeurObservee.Texte("Outil Exemple"),
        });

        var resultat = new FournisseurMsi().Extraire(fichier);

        Assert.DoesNotContain(resultat.Preuves, p => p.Dimension == DimensionPreuveVersionnee.CleFamille);
        Assert.Contains(resultat.Diagnostics, d => d.Code == CodeDiagnosticVersionne.AttributInvalide);
    }

    [Fact]
    public void Appx_extrait_une_cle_native_deterministe_et_son_architecture()
    {
        var attributs = new Dictionary<Attribut, ValeurObservee>
        {
            [new("appx_manifest", "name")] = new ValeurObservee.Texte("Contoso.Outil"),
            [new("appx_manifest", "publisher")] = new ValeurObservee.Texte("CN=Contoso"),
            [new("appx_manifest", "version")] = new ValeurObservee.Texte("3.2.1.0"),
            [new("appx_manifest", "processor_architecture")] = new ValeurObservee.Texte("AMD64"),
        };

        var premier = new FournisseurAppx().Extraire(Fichier(@"C:\corpus\outil.msix", attributs));
        var second = new FournisseurAppx().Extraire(Fichier(@"D:\archives\copie.msix", attributs));

        var cle1 = Preuve(premier, DimensionPreuveVersionnee.CleFamille).ValeurNormalisee;
        var cle2 = Preuve(second, DimensionPreuveVersionnee.CleFamille).ValeurNormalisee;
        Assert.Equal(cle1, cle2);
        Assert.Matches("^appx-family:[0-9a-f]{64}$", cle1);
        AssertPreuve(premier, DimensionPreuveVersionnee.LibelleFamille, "CONTOSO.OUTIL");
        AssertPreuve(premier, DimensionPreuveVersionnee.Editeur, "CN=CONTOSO");
        AssertPreuve(premier, DimensionPreuveVersionnee.Version, "3.2.1");
        AssertPreuve(premier, DimensionPreuveVersionnee.Architecture, "x64");
        AssertPreuve(premier, DimensionPreuveVersionnee.Format, ".msix");
        Assert.Empty(premier.Diagnostics);
    }

    [Theory]
    [InlineData(@"C:\corpus\outil.appx", ".appx")]
    [InlineData(@"C:\corpus\outil.msixbundle", ".msixbundle")]
    [InlineData(@"C:\corpus\outil.bin", "<appx-package>")]
    public void Appx_conserve_le_type_reel_du_paquet(string chemin, string format)
    {
        var fichier = Fichier(chemin, new Dictionary<Attribut, ValeurObservee>
        {
            [new("appx_manifest", "name")] = new ValeurObservee.Texte("Contoso.Outil"),
        });

        var resultat = new FournisseurAppx().Extraire(fichier);

        AssertPreuve(resultat, DimensionPreuveVersionnee.Format, format);
    }

    [Fact]
    public void Un_manifeste_Appx_incomplet_ne_produit_pas_de_cle_native()
    {
        var fichier = Fichier(@"C:\corpus\outil.appx", new Dictionary<Attribut, ValeurObservee>
        {
            [new("appx_manifest", "name")] = new ValeurObservee.Texte("Contoso.Outil"),
            [new("appx_manifest", "version")] = new ValeurObservee.Texte("1.0"),
        });

        var resultat = new FournisseurAppx().Extraire(fichier);

        Assert.DoesNotContain(resultat.Preuves, p => p.Dimension == DimensionPreuveVersionnee.CleFamille);
        AssertPreuve(resultat, DimensionPreuveVersionnee.Version, "1");
    }

    private static FichierObserve Fichier(
        string chemin,
        IReadOnlyDictionary<Attribut, ValeurObservee> attributs) =>
        new(
            1,
            IdentifiantsStables.PourFichier(Hash, chemin),
            chemin,
            100,
            Hash,
            attributs);

    private static PreuveVersionnee Preuve(
        ResultatFournisseur resultat,
        DimensionPreuveVersionnee dimension) =>
        Assert.Single(resultat.Preuves, p => p.Dimension == dimension);

    private static void AssertPreuve(
        ResultatFournisseur resultat,
        DimensionPreuveVersionnee dimension,
        string valeur) =>
        Assert.Equal(valeur, Preuve(resultat, dimension).ValeurNormalisee);
}
