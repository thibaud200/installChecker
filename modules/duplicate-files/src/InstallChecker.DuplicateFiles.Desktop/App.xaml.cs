using System.IO;
using System.Windows;
using InstallChecker.DuplicateFiles.Desktop.Adaptateurs;
using InstallChecker.DuplicateFiles.Desktop.Bibliotheque;
using InstallChecker.DuplicateFiles.Desktop.Infrastructure;
using InstallChecker.DuplicateFiles.Desktop.Presentation;
using InstallChecker.DuplicateFiles.Desktop.Session;
using InstallChecker.DuplicateFiles.Desktop.ViewModels;

namespace InstallChecker.DuplicateFiles.Desktop;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var viewModel = new MainViewModel(
            new StockageSessionUi(),
            new ValidateurRacines(),
            new ScannerBibliotheque(),
            new AnalyseurBibliotheque(),
            new LecteurRapportDoublonsUi(),
            new LecteurRapportVersionsUi(),
            new DialogueFichiersWpf(),
            new DispatcherUiWpf(),
            TrouverRegistre());
        var fenetre = new MainWindow { DataContext = viewModel };
        MainWindow = fenetre;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        fenetre.Show();

        try
        {
            var source = LireSourceInitiale(e.Args);
            if (source is not null)
                await ChargerSourceInitialeAsync(viewModel, source.Value);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
            or InvalidDataException or ArgumentException or System.Text.Json.JsonException)
        {
            MessageBox.Show(
                fenetre,
                ex.Message,
                "Impossible d'ouvrir la source",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private static async Task ChargerSourceInitialeAsync(
        MainViewModel viewModel,
        SourceInitiale source)
    {
        switch (source.Option)
        {
            case "--session":
                await viewModel.OuvrirSessionAsync(source.Chemin);
                break;
            case "--db":
                await viewModel.OuvrirBaseAsync(source.Chemin);
                break;
            case "--json":
                await viewModel.ImporterRapportAsync(source.Chemin);
                break;
        }
    }

    private static SourceInitiale? LireSourceInitiale(IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
            return null;
        if (arguments.Count != 2)
            throw new ArgumentException("Utilisez une seule source : --session, --db ou --json suivie de son fichier.");

        var option = arguments[0];
        if (option is not ("--session" or "--db" or "--json"))
            throw new ArgumentException($"Option inconnue : {option}.");

        return new SourceInitiale(option, arguments[1]);
    }

    private static string TrouverRegistre()
    {
        var depuisCourant = Path.Combine(Environment.CurrentDirectory, "registre");
        if (Directory.Exists(depuisCourant))
            return depuisCourant;

        var courant = new DirectoryInfo(AppContext.BaseDirectory);
        while (courant is not null)
        {
            var candidat = Path.Combine(courant.FullName, "registre");
            if (Directory.Exists(candidat))
                return candidat;

            courant = courant.Parent;
        }

        return depuisCourant;
    }

    private readonly record struct SourceInitiale(string Option, string Chemin);
}
