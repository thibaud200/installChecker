using InstallChecker;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

public sealed class StockageV3Tests : IDisposable
{
    private readonly string _racine = Directory.CreateTempSubdirectory("scanner-v3-source-").FullName;
    private readonly string _baseDir = Directory.CreateTempSubdirectory("scanner-v3-db-").FullName;
    private string BasePath => Path.Combine(_baseDir, "observations.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_racine, recursive: true);
        Directory.Delete(_baseDir, recursive: true);
    }

    [Fact]
    public void Deux_chemins_de_meme_contenu_creent_un_snapshot_et_deux_occurrences()
    {
        File.WriteAllText(Path.Combine(_racine, "a.bin"), "contenu identique");
        File.WriteAllText(Path.Combine(_racine, "b.bin"), "contenu identique");

        Assert.Equal(0, Scanner());

        using var connexion = OuvrirBase();
        Assert.Equal(3L, Scalaire(connexion, "PRAGMA user_version;"));
        Assert.Equal(1L, Scalaire(connexion, "SELECT COUNT(*) FROM observation_snapshots;"));
        Assert.Equal(2L, Scalaire(connexion, "SELECT COUNT(*) FROM scan_entries;"));
        Assert.Equal(1L, Scalaire(connexion, "SELECT COUNT(DISTINCT snapshot_id) FROM scan_entries;"));
    }

    [Fact]
    public void Rescanner_les_memes_chemins_reutilise_le_snapshot_et_conserve_les_occurrences_historiques()
    {
        File.WriteAllText(Path.Combine(_racine, "a.bin"), "contenu identique");
        File.WriteAllText(Path.Combine(_racine, "b.bin"), "contenu identique");

        Assert.Equal(0, Scanner());
        Assert.Equal(0, Scanner());

        using var connexion = OuvrirBase();
        Assert.Equal(2L, Scalaire(connexion, "SELECT COUNT(*) FROM scans;"));
        Assert.Equal(1L, Scalaire(connexion, "SELECT COUNT(*) FROM observation_snapshots;"));
        Assert.Equal(4L, Scalaire(connexion, "SELECT COUNT(*) FROM scan_entries;"));
    }

    private int Scanner() =>
        ScanCommand.Run(_racine, BasePath, false, TextWriter.Null, TextWriter.Null);

    private SqliteConnection OuvrirBase()
    {
        var connexion = new SqliteConnection($"Data Source={BasePath}");
        connexion.Open();
        return connexion;
    }

    private static long Scalaire(SqliteConnection connexion, string sql)
    {
        using var commande = connexion.CreateCommand();
        commande.CommandText = sql;
        return (long)commande.ExecuteScalar()!;
    }
}
