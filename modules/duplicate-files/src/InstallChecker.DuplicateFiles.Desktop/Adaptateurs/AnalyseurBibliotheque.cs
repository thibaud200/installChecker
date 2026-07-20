using System.IO;
using System.Text.Json;
using InstallChecker.DuplicateFiles.Desktop.Session;

namespace InstallChecker.DuplicateFiles.Desktop.Adaptateurs;

public delegate int AnalyserDoublons(
    string cheminBase,
    string cheminRegistre,
    TextWriter output,
    TextWriter errors);

public delegate int AnalyserVersions(string cheminBase, TextWriter output, TextWriter errors);

public sealed record ResultatAnalyseUi(
    bool Reussi,
    JsonElement? RapportDoublons,
    JsonElement? RapportVersions,
    IReadOnlyList<DiagnosticUi> Diagnostics);

public interface IAnalyseurBibliotheque
{
    Task<ResultatAnalyseUi> AnalyserAsync(
        BibliothequeUi bibliotheque,
        CancellationToken cancellationToken);
}

public sealed class AnalyseurBibliotheque : IAnalyseurBibliotheque
{
    private readonly AnalyserDoublons _analyserDoublons;
    private readonly AnalyserVersions _analyserVersions;

    public AnalyseurBibliotheque(
        AnalyserDoublons? analyserDoublons = null,
        AnalyserVersions? analyserVersions = null)
    {
        _analyserDoublons = analyserDoublons ?? DuplicatesCommand.Deriver;
        _analyserVersions = analyserVersions ?? RedondanceVersionneeCommand.Deriver;
    }

    public Task<ResultatAnalyseUi> AnalyserAsync(
        BibliothequeUi bibliotheque,
        CancellationToken cancellationToken) =>
        Task.Run(() => Analyser(bibliotheque), cancellationToken);

    private ResultatAnalyseUi Analyser(BibliothequeUi bibliotheque)
    {
        var diagnostics = new List<DiagnosticUi>();
        var (codeDoublons, jsonDoublons) = Executer(
            (output, errors) => _analyserDoublons(
                bibliotheque.CheminBase,
                bibliotheque.CheminRegistre,
                output,
                errors),
            "AnalyseDoublonsEchouee",
            diagnostics);
        var (codeVersions, jsonVersions) = Executer(
            (output, errors) => _analyserVersions(
                bibliotheque.CheminBase,
                output,
                errors),
            "AnalyseVersionsEchouee",
            diagnostics);

        return new ResultatAnalyseUi(
            codeDoublons == 0 && jsonDoublons is not null
                && codeVersions == 0 && jsonVersions is not null,
            jsonDoublons,
            jsonVersions,
            diagnostics);
    }

    private static (int Code, JsonElement? Json) Executer(
        Func<TextWriter, TextWriter, int> commande,
        string codeDiagnostic,
        ICollection<DiagnosticUi> diagnostics)
    {
        using var output = new StringWriter();
        using var errors = new StringWriter();
        var code = commande(output, errors);

        if (code != 0)
        {
            diagnostics.Add(new DiagnosticUi(
                codeDiagnostic,
                string.IsNullOrWhiteSpace(errors.ToString())
                    ? "La commande d'analyse a échoué."
                    : errors.ToString().Trim()));
            return (code, null);
        }

        try
        {
            using var document = JsonDocument.Parse(output.ToString());
            return (code, document.RootElement.Clone());
        }
        catch (JsonException ex)
        {
            diagnostics.Add(new DiagnosticUi(
                $"{codeDiagnostic}Json",
                $"La commande a produit un JSON invalide : {ex.Message}"));
            return (1, null);
        }
    }
}
