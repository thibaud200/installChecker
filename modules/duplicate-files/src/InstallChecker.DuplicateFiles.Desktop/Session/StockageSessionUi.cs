using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InstallChecker.DuplicateFiles.Desktop.Session;

public interface IStockageSessionUi
{
    Task<SessionDuplicateFilesUi> ChargerAsync(string chemin, CancellationToken cancellationToken);

    Task SauvegarderAsync(
        string cheminCourant,
        SessionDuplicateFilesUi session,
        bool tournerArchive,
        CancellationToken cancellationToken);
}

public sealed class StockageSessionUi : IStockageSessionUi
{
    private static readonly JsonSerializerOptions Options = CreerOptions();
    private readonly Func<string, SessionDuplicateFilesUi, CancellationToken, Task> _ecrireTemporaire;

    public StockageSessionUi(
        Func<string, SessionDuplicateFilesUi, CancellationToken, Task>? ecrireTemporaire = null)
    {
        _ecrireTemporaire = ecrireTemporaire ?? EcrireTemporaireAsync;
    }

    public async Task<SessionDuplicateFilesUi> ChargerAsync(
        string chemin,
        CancellationToken cancellationToken)
    {
        await using var flux = new FileStream(
            chemin,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            65_536,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var session = await JsonSerializer.DeserializeAsync<SessionDuplicateFilesUi>(
            flux,
            Options,
            cancellationToken);

        if (session is null || session.VersionContrat != VersionsContratUi.SessionV1)
        {
            throw new InvalidDataException(
                $"Version de session non prise en charge. Version attendue : {VersionsContratUi.SessionV1}.");
        }

        return session;
    }

    public async Task SauvegarderAsync(
        string cheminCourant,
        SessionDuplicateFilesUi session,
        bool tournerArchive,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cheminCourant);

        var cheminComplet = Path.GetFullPath(cheminCourant);
        var repertoire = Path.GetDirectoryName(cheminComplet)
            ?? throw new InvalidOperationException("Le chemin de session ne possède pas de répertoire.");
        Directory.CreateDirectory(repertoire);

        var temporaire = $"{cheminComplet}.tmp-{Guid.NewGuid():N}";
        try
        {
            await _ecrireTemporaire(temporaire, session, cancellationToken);
            await ValiderJsonAsync(temporaire, cancellationToken);

            if (!File.Exists(cheminComplet))
            {
                File.Move(temporaire, cheminComplet);
                return;
            }

            if (tournerArchive)
            {
                var archive = CheminArchive(cheminComplet);
                if (File.Exists(archive))
                    File.Delete(archive);

                File.Replace(temporaire, cheminComplet, archive, ignoreMetadataErrors: true);
                return;
            }

            File.Replace(temporaire, cheminComplet, null, ignoreMetadataErrors: true);
        }
        finally
        {
            if (File.Exists(temporaire))
                File.Delete(temporaire);
        }
    }

    public static string CheminArchive(string cheminCourant)
    {
        var cheminComplet = Path.GetFullPath(cheminCourant);
        var repertoire = Path.GetDirectoryName(cheminComplet)
            ?? throw new InvalidOperationException("Le chemin de session ne possède pas de répertoire.");
        var nomSansExtension = Path.GetFileNameWithoutExtension(cheminComplet);
        return Path.Combine(repertoire, $"{nomSansExtension}.previous.json");
    }

    private static async Task EcrireTemporaireAsync(
        string chemin,
        SessionDuplicateFilesUi session,
        CancellationToken cancellationToken)
    {
        await using var flux = new FileStream(
            chemin,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            65_536,
            FileOptions.Asynchronous);

        await JsonSerializer.SerializeAsync(flux, session, Options, cancellationToken);
        await flux.FlushAsync(cancellationToken);
    }

    private static async Task ValiderJsonAsync(string chemin, CancellationToken cancellationToken)
    {
        await using var flux = new FileStream(
            chemin,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            65_536,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var _ = await JsonDocument.ParseAsync(flux, cancellationToken: cancellationToken);
    }

    private static JsonSerializerOptions CreerOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
