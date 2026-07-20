using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class IdentifiantsStablesTests
{
    private const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string HashB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    [Fact]
    public void Lidentifiant_de_groupe_normalise_le_sha256_en_minuscules()
    {
        var identifiant = IdentifiantsStables.PourGroupeExact(HashA.ToUpperInvariant());

        Assert.Equal($"exact:sha256:{HashA}", identifiant);
    }

    [Fact]
    public void Lidentifiant_de_fichier_suit_la_semantique_de_chemin_Windows()
    {
        var premier = IdentifiantsStables.PourFichier(HashA, @"C:\Archives\Setup.exe");
        var second = IdentifiantsStables.PourFichier(HashA, @"c:/archives/setup.exe");

        Assert.Equal(premier, second);
        Assert.StartsWith("file:sha256:", premier);
        Assert.Equal("file:sha256:".Length + 64, premier.Length);
    }

    [Fact]
    public void Lidentifiant_de_fichier_change_avec_le_chemin_ou_le_contenu()
    {
        var reference = IdentifiantsStables.PourFichier(HashA, @"C:\Archives\Setup.exe");

        Assert.NotEqual(reference, IdentifiantsStables.PourFichier(HashA, @"D:\Archives\Setup.exe"));
        Assert.NotEqual(reference, IdentifiantsStables.PourFichier(HashB, @"C:\Archives\Setup.exe"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("pas-un-sha256")]
    [InlineData("gggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg")]
    public void Une_empreinte_invalide_est_refusee(string empreinte)
    {
        Assert.Throws<ArgumentException>(() => IdentifiantsStables.PourGroupeExact(empreinte));
    }

    [Fact]
    public void Lidentifiant_versionne_est_stable_et_independant_de_la_valeur_de_version()
    {
        var premier = IdentifiantsStables.PourGroupeVersionne(
            "msi",
            "12345678-1234-1234-1234-1234567890ab",
            SchemaVersionComparable.Numerique,
            ".msi",
            null,
            "1036",
            null,
            null);
        var second = IdentifiantsStables.PourGroupeVersionne(
            "msi",
            "12345678-1234-1234-1234-1234567890ab",
            SchemaVersionComparable.Numerique,
            ".msi",
            null,
            "1036",
            null,
            null);

        Assert.Equal(premier, second);
        Assert.Matches("^version:sha256:[0-9a-f]{64}$", premier);
    }

    [Fact]
    public void Lidentifiant_versionne_change_avec_le_schema_ou_une_variante()
    {
        var reference = IdentifiantsStables.PourGroupeVersionne(
            "filename",
            "OUTIL",
            SchemaVersionComparable.Numerique,
            ".zip",
            "x64",
            null,
            null,
            null);

        Assert.NotEqual(reference, IdentifiantsStables.PourGroupeVersionne(
            "filename", "OUTIL", SchemaVersionComparable.Calendaire, ".zip", "x64", null, null, null));
        Assert.NotEqual(reference, IdentifiantsStables.PourGroupeVersionne(
            "filename", "OUTIL", SchemaVersionComparable.Numerique, ".zip", "arm64", null, null, null));
    }
}
