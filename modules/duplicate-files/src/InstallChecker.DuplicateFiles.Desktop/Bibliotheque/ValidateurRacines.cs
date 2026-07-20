using System.IO;
using InstallChecker.DuplicateFiles.Desktop.Session;

namespace InstallChecker.DuplicateFiles.Desktop.Bibliotheque;

public sealed record RacineValidee(string Chemin, string VolumeId);

public sealed record ResultatValidationRacines(
    bool EstValide,
    IReadOnlyList<string> Racines,
    IReadOnlyList<DiagnosticUi> Diagnostics);

public interface IValidateurRacines
{
    ResultatValidationRacines Valider(IEnumerable<string> chemins);
}

public sealed class ValidateurRacines : IValidateurRacines
{
    private readonly Func<string, string> _resoudreVolume;

    public ValidateurRacines()
        : this(chemin => VolumeIdentityExtractor.Resolve(chemin).VolumeId)
    {
    }

    public ValidateurRacines(Func<string, string> resoudreVolume)
    {
        _resoudreVolume = resoudreVolume;
    }

    public ResultatValidationRacines Valider(IEnumerable<string> chemins)
    {
        var diagnostics = new List<DiagnosticUi>();
        var racines = chemins
            .Where(chemin => !string.IsNullOrWhiteSpace(chemin))
            .Select(Normaliser)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var resolues = new List<RacineValidee>();
        foreach (var racine in racines)
        {
            try
            {
                resolues.Add(new RacineValidee(racine, _resoudreVolume(racine)));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                or InvalidOperationException or ArgumentException)
            {
                diagnostics.Add(new DiagnosticUi(
                    "VolumeIrresoluble",
                    $"Impossible d'identifier le volume : {ex.Message}",
                    racine));
            }
        }

        var retenues = new List<RacineValidee>();
        foreach (var volume in resolues.GroupBy(r => r.VolumeId, StringComparer.OrdinalIgnoreCase))
        {
            var independantes = new List<RacineValidee>();
            foreach (var racine in volume.OrderBy(r => r.Chemin.Length))
            {
                if (independantes.Any(parent => EstMemeCheminOuDescendant(racine.Chemin, parent.Chemin)))
                    continue;

                independantes.Add(racine);
            }

            if (independantes.Count > 1)
            {
                diagnostics.Add(new DiagnosticUi(
                    "RacinesMemeVolume",
                    "Deux dossiers indépendants du même lecteur ne peuvent pas être scannés ensemble dans cette version.",
                    string.Join(" | ", independantes.Select(r => r.Chemin))));
            }

            retenues.AddRange(independantes);
        }

        return new ResultatValidationRacines(
            diagnostics.Count == 0,
            retenues.Select(r => r.Chemin).ToArray(),
            diagnostics);
    }

    private static string Normaliser(string chemin)
    {
        var complet = Path.GetFullPath(chemin);
        var racine = Path.GetPathRoot(complet);
        if (string.Equals(complet, racine, StringComparison.OrdinalIgnoreCase))
            return complet;

        return complet.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool EstMemeCheminOuDescendant(string chemin, string parent)
    {
        if (string.Equals(chemin, parent, StringComparison.OrdinalIgnoreCase))
            return true;

        var prefixe = parent.EndsWith(Path.DirectorySeparatorChar)
            || parent.EndsWith(Path.AltDirectorySeparatorChar)
                ? parent
                : parent + Path.DirectorySeparatorChar;
        return chemin.StartsWith(prefixe, StringComparison.OrdinalIgnoreCase);
    }
}
