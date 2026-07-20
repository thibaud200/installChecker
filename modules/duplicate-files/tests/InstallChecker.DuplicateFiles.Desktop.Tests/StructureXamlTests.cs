namespace InstallChecker.DuplicateFiles.Desktop.Tests;

public sealed class StructureXamlTests
{
    [Fact]
    public void Grilles_de_resultats_activent_la_virtualisation()
    {
        var xaml = File.ReadAllText(CheminSource("MainWindow.xaml"));

        Assert.Contains("VirtualizingPanel.IsVirtualizing=\"True\"", xaml);
        Assert.Contains("VirtualizingPanel.VirtualizationMode=\"Recycling\"", xaml);
        Assert.DoesNotContain("CanUserAddRows=\"True\"", xaml);
    }

    [Fact]
    public void Suppression_est_visible_et_explicitement_desactivee()
    {
        var xaml = File.ReadAllText(CheminSource("MainWindow.xaml"));

        Assert.Contains("Content=\"Supprimer\"", xaml);
        Assert.Contains("Command=\"{Binding SupprimerCommand}\"", xaml);
        Assert.Contains("Exécution non disponible", xaml);
    }

    [Fact]
    public void Interface_expose_les_deux_domaines_fonctionnels()
    {
        var xaml = File.ReadAllText(CheminSource("MainWindow.xaml"));

        Assert.Contains("Header=\"Doublons exacts\"", xaml);
        Assert.Contains("Header=\"Versions apparentées\"", xaml);
        Assert.Contains("Binding Confiance", xaml);
        Assert.Contains("Binding Architecture", xaml);
        Assert.Contains("Revue humaine", xaml);
    }

    [Fact]
    public void App_compose_le_ViewModel_sans_conteneur_externe()
    {
        var source = File.ReadAllText(CheminSource("App.xaml.cs"));

        Assert.Contains("new StockageSessionUi", source);
        Assert.Contains("new ScannerBibliotheque", source);
        Assert.Contains("new AnalyseurBibliotheque", source);
        Assert.Contains("new MainViewModel", source);
        Assert.DoesNotContain("ServiceCollection", source);
    }

    private static string CheminSource(string fichier)
    {
        var courant = new DirectoryInfo(AppContext.BaseDirectory);
        while (courant is not null)
        {
            var candidat = Path.Combine(
                courant.FullName,
                "modules",
                "duplicate-files",
                "src",
                "InstallChecker.DuplicateFiles.Desktop",
                fichier);
            if (File.Exists(candidat))
                return candidat;

            courant = courant.Parent;
        }

        throw new FileNotFoundException(fichier);
    }
}
