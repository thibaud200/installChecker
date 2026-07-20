using InstallChecker;

namespace InstallChecker.Tests;

public sealed class CleSnapshotObservationTests
{
    [Fact]
    public void Meme_contenu_brut_a_un_autre_chemin_et_une_autre_date_conserve_la_meme_cle()
    {
        var premiere = Observation(path: @"C:\\a\\outil.exe", scannedAt: "2026-07-19T10:00:00Z");
        var seconde = premiere with { Path = @"D:\\archives\\copie.exe", ScannedAt = "2026-07-20T11:00:00Z" };

        var clePremiere = CleSnapshotObservation.Calculer(premiere);
        var cleSeconde = CleSnapshotObservation.Calculer(seconde);

        Assert.Equal(clePremiere.Cle, cleSeconde.Cle);
        Assert.Equal(clePremiere.ChargeCanonique, cleSeconde.ChargeCanonique);
    }

    [Fact]
    public void Toute_valeur_brute_participe_a_la_cle()
    {
        var originale = Observation();
        var modifiee = originale with
        {
            MsiProperties = originale.MsiProperties with { ProductLanguage = "1036" },
        };

        Assert.NotEqual(
            CleSnapshotObservation.Calculer(originale).Cle,
            CleSnapshotObservation.Calculer(modifiee).Cle);
    }

    [Fact]
    public void Null_et_chaine_vide_restent_deux_observations_distinctes()
    {
        var sansValeur = Observation() with
        {
            VersionInfo = new VersionInfoObservation(null, "Editeur", "1.2.3", "1.2.3.0"),
        };
        var chaineVide = sansValeur with
        {
            VersionInfo = sansValeur.VersionInfo with { ProductName = string.Empty },
        };

        Assert.NotEqual(
            CleSnapshotObservation.Calculer(sansValeur).Cle,
            CleSnapshotObservation.Calculer(chaineVide).Cle);
    }

    [Fact]
    public void Cle_est_un_sha256_minuscule_et_versionne()
    {
        var resultat = CleSnapshotObservation.Calculer(Observation());

        Assert.Matches("^snapshot:sha256:[0-9a-f]{64}$", resultat.Cle);
        Assert.NotEmpty(resultat.ChargeCanonique);
        Assert.Equal("scanner-observation/v1", CleSnapshotObservation.VersionContrat);
    }

    private static FileObservation Observation(
        string path = @"C:\\a\\outil.exe",
        string scannedAt = "2026-07-19T10:00:00Z") =>
        new(
            Path: path,
            Size: 123,
            Sha256: new string('a', 64),
            ScannedAt: scannedAt,
            MagicHex: "4d5a",
            Container: "PE",
            VersionInfo: new VersionInfoObservation("Outil", "Editeur", "1.2.3", "1.2.3.0"),
            PeInfo: new PeInfoExtractor.PeInfo("8664", "0002", 34, 123456, "020b"),
            Authenticode: new AuthenticodeExtractor.AuthenticodeInfo("CN=Editeur", "CN=CA", "01", "AB", "avant", "apres"),
            MsiProperties: new MsiPropertiesExtractor.MsiProperties("Outil", "1.2.3", "Editeur", "P", "U", null),
            AppxManifest: new AppxManifestExtractor.AppxManifest("Outil.App", "CN=Editeur", "1.2.3.0", "x64"));
}
