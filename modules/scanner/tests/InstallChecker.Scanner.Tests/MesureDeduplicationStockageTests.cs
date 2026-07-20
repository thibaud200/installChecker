using System.Diagnostics;
using InstallChecker;
using Microsoft.Data.Sqlite;
using Xunit.Abstractions;

namespace InstallChecker.Tests;

public sealed class MesureDeduplicationStockageTests(ITestOutputHelper output) : IDisposable
{
    private readonly string _repertoire = Directory.CreateTempSubdirectory("scanner-mesure-v3-").FullName;
    private string BasePath => Path.Combine(_repertoire, "mesure.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_repertoire, recursive: true);
    }

    [Fact]
    [Trait("Category", "Performance")]
    public void Cent_mille_occurrences_mutualisent_dix_mille_snapshots()
    {
        var allocationsAvant = GC.GetAllocatedBytesForCurrentThread();
        var chrono = Stopwatch.StartNew();

        for (var scan = 0; scan < 10; scan++)
        {
            using var store = new ObservationStore(
                BasePath,
                new ScanDeclaration("volume-mesure", null, @"C:\\corpus", $"scan-{scan}", null));
            for (var index = 0; index < 10_000; index++)
                store.Persist(Observation(index, scan));
            store.Commit();
        }

        chrono.Stop();
        var allocations = GC.GetAllocatedBytesForCurrentThread() - allocationsAvant;
        var tailleBase = new FileInfo(BasePath).Length;

        using var connexion = new SqliteConnection($"Data Source={BasePath}");
        connexion.Open();
        Assert.Equal(10_000L, Compter(connexion, "observation_snapshots"));
        Assert.Equal(100_000L, Compter(connexion, "scan_entries"));
        Assert.Equal(10_000L, Compter(connexion, "snapshot_version_info"));

        output.WriteLine($"Durée: {chrono.Elapsed}");
        output.WriteLine($"Allocations thread: {allocations:N0} octets");
        output.WriteLine($"Taille SQLite: {tailleBase:N0} octets");
        output.WriteLine($"Machine: {Environment.MachineName}, {Environment.OSVersion}, .NET {Environment.Version}");
    }

    private static FileObservation Observation(int index, int scan) =>
        new(
            Path: $@"C:\\corpus\\fichier-{index:D5}.bin",
            Size: index,
            Sha256: index.ToString("x64"),
            ScannedAt: $"scan-{scan}",
            MagicHex: "00",
            Container: null,
            VersionInfo: new VersionInfoObservation(null, null, index.ToString(), null),
            PeInfo: PeInfoExtractor.PeInfo.None,
            Authenticode: AuthenticodeExtractor.AuthenticodeInfo.None,
            MsiProperties: MsiPropertiesExtractor.MsiProperties.None,
            AppxManifest: AppxManifestExtractor.AppxManifest.None);

    private static long Compter(SqliteConnection connexion, string table)
    {
        using var commande = connexion.CreateCommand();
        commande.CommandText = $"SELECT COUNT(*) FROM {table};";
        return (long)commande.ExecuteScalar()!;
    }
}
