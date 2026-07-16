namespace InstallChecker.Tests;

public class VolumeIdentityExtractorTests
{
    // La branche UNC est du pur calcul de chaîne : aucune I/O réseau, testable partout.
    [Theory]
    [InlineData(@"\\SERVEUR\Partage\sous\dossier", @"\\serveur\partage")]
    [InlineData(@"\\nas\Archives", @"\\nas\archives")]
    [InlineData(@"\\nas\Archives\", @"\\nas\archives")]
    public void Une_racine_UNC_est_normalisee_en_minuscules_sur_serveur_partage(string chemin, string attendu)
    {
        var identite = VolumeIdentityExtractor.Resolve(chemin);

        Assert.Equal(attendu, identite.VolumeId);
        Assert.Null(identite.VolumeLabel);
    }

    [Fact]
    public void Un_chemin_UNC_sans_partage_est_refuse_explicitement()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => VolumeIdentityExtractor.Resolve(@"\\serveur"));
        Assert.StartsWith("Erreur : identité de volume irrésoluble", ex.Message);
    }

    [Fact]
    public void Un_chemin_local_produit_une_serie_hexadecimale_de_8_caracteres()
    {
        var identite = VolumeIdentityExtractor.Resolve(Path.GetTempPath());

        Assert.Matches("^[0-9a-f]{8}$", identite.VolumeId);
    }
}
