using System.IO;
using InstallChecker.DuplicateFiles.Desktop.Session;

namespace InstallChecker.DuplicateFiles.Desktop.Adaptateurs;

public delegate int ExecuterScan(
    string racine,
    string cheminBase,
    bool sortieJson,
    TextWriter output,
    TextWriter errors,
    IReadOnlyCollection<string>? extensions,
    string? cheminJson);

public sealed record ResultatScanUi(
    bool Reussi,
    bool Partiel,
    long FichiersTraites,
    IReadOnlyList<DiagnosticUi> Diagnostics);

public interface IScannerBibliotheque
{
    Task<ResultatScanUi> ExecuterAsync(
        BibliothequeUi bibliotheque,
        IProgress<ProgressionScanUi>? progression,
        CancellationToken cancellationToken);
}

public sealed class ScannerBibliotheque : IScannerBibliotheque
{
    private readonly ExecuterScan _executerScan;

    public ScannerBibliotheque(ExecuterScan? executerScan = null)
    {
        _executerScan = executerScan ?? ScanCommand.Run;
    }

    public Task<ResultatScanUi> ExecuterAsync(
        BibliothequeUi bibliotheque,
        IProgress<ProgressionScanUi>? progression,
        CancellationToken cancellationToken) =>
        Task.Run(() => Executer(bibliotheque, progression, cancellationToken));

    private ResultatScanUi Executer(
        BibliothequeUi bibliotheque,
        IProgress<ProgressionScanUi>? progression,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<DiagnosticUi>();
        var succes = 0;
        var echecs = 0;
        var interrompu = false;
        using var output = new ProgressionScanTextWriter(p => progression?.Report(p));

        foreach (var racine in bibliotheque.Racines)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                interrompu = true;
                diagnostics.Add(new DiagnosticUi(
                    "ScanNonDemarre",
                    "Le scan de cette racine n'a pas démarré.",
                    racine));
                break;
            }

            using var errors = new StringWriter();
            var code = _executerScan(
                racine,
                bibliotheque.CheminBase,
                false,
                output,
                errors,
                null,
                null);

            if (code == 0)
            {
                succes++;
                AjouterDiagnosticsLocaux(errors.ToString(), racine, diagnostics);
                continue;
            }

            echecs++;
            diagnostics.Add(new DiagnosticUi(
                "ScanRacineEchoue",
                MessageOuDefaut(errors.ToString(), "Le scan de la racine a échoué."),
                racine));
        }

        var reussi = bibliotheque.Racines.Count > 0 && echecs == 0 && !interrompu;
        var partiel = succes > 0 && (echecs > 0 || interrompu);
        return new ResultatScanUi(reussi, partiel, output.FichiersTraites, diagnostics);
    }

    private static void AjouterDiagnosticsLocaux(
        string erreurs,
        string racine,
        ICollection<DiagnosticUi> diagnostics)
    {
        foreach (var ligne in erreurs.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            if (ligne.StartsWith("Scan terminé", StringComparison.OrdinalIgnoreCase))
                continue;

            diagnostics.Add(new DiagnosticUi("ScanAvertissement", ligne.Trim(), racine));
        }
    }

    private static string MessageOuDefaut(string message, string valeurParDefaut) =>
        string.IsNullOrWhiteSpace(message) ? valeurParDefaut : message.Trim();
}
