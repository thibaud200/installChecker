using InstallChecker.DuplicateFiles.Desktop.Adaptateurs;

namespace InstallChecker.DuplicateFiles.Desktop.Tests;

public sealed class ProgressionScanTextWriterTests
{
    [Fact]
    public void Une_ligne_TSV_publie_un_fichier_traite()
    {
        ProgressionScanUi? derniere = null;
        using var writer = new ProgressionScanTextWriter(p => derniere = p);

        writer.WriteLine("D:\\a.exe\t12\tabcd");

        Assert.Equal(1, derniere!.FichiersTraites);
        Assert.Equal(@"D:\a.exe", derniere.CheminCourant);
    }

    [Fact]
    public void Une_ligne_non_TSV_est_ignoree()
    {
        ProgressionScanUi? derniere = null;
        using var writer = new ProgressionScanTextWriter(p => derniere = p);

        writer.WriteLine("message sans tabulation");

        Assert.Null(derniere);
        Assert.Equal(0, writer.FichiersTraites);
    }
}
