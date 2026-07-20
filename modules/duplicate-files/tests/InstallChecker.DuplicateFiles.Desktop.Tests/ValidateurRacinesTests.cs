using InstallChecker.DuplicateFiles.Desktop.Bibliotheque;

namespace InstallChecker.DuplicateFiles.Desktop.Tests;

public sealed class ValidateurRacinesTests
{
    [Fact]
    public void Deux_lecteurs_sont_acceptes()
    {
        var validateur = new ValidateurRacines(
            chemin => chemin.StartsWith("C:", StringComparison.OrdinalIgnoreCase)
                ? "volume-c"
                : "volume-d");

        var resultat = validateur.Valider([@"C:\Corpus", @"D:\Archives"]);

        Assert.True(resultat.EstValide);
        Assert.Equal(2, resultat.Racines.Count);
    }

    [Fact]
    public void Sous_dossier_deja_couvert_est_retire()
    {
        var resultat = new ValidateurRacines(_ => "volume-c")
            .Valider([@"C:\Corpus", @"C:\Corpus\Sous"]);

        Assert.True(resultat.EstValide);
        Assert.Equal([@"C:\Corpus"], resultat.Racines);
    }

    [Fact]
    public void Deux_racines_independantes_du_meme_volume_sont_refusees()
    {
        var resultat = new ValidateurRacines(_ => "volume-d")
            .Valider([@"D:\Photos", @"D:\Archives"]);

        Assert.False(resultat.EstValide);
        Assert.Contains(resultat.Diagnostics, d => d.Code == "RacinesMemeVolume");
    }

    [Fact]
    public void Racine_invalide_devient_un_diagnostic_explicite()
    {
        var resultat = new ValidateurRacines(_ => throw new InvalidOperationException("volume absent"))
            .Valider([@"Z:\Indisponible"]);

        Assert.False(resultat.EstValide);
        Assert.Contains(resultat.Diagnostics, d =>
            d.Code == "VolumeIrresoluble" && d.Chemin == @"Z:\Indisponible");
    }
}
