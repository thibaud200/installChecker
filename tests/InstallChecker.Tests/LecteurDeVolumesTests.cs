using InstallChecker.DuplicateFiles;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

public class LecteurDeVolumesTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("installchecker-volumes-").FullName;
    private readonly string _dbDir = Directory.CreateTempSubdirectory("installchecker-volumes-db-").FullName;
    private string DbPath => Path.Combine(_dbDir, "test.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
        Directory.Delete(_dbDir, recursive: true);
    }

    [Fact]
    public void Une_base_v2_donne_le_volume_du_scan_pour_chaque_observation_courante()
    {
        File.WriteAllText(Path.Combine(_root, "a.exe"), "x");
        Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));

        var volumes = LecteurDeVolumes.Lire(DbPath);

        var volume = Assert.Single(volumes).Value;
        Assert.Matches("^[0-9a-f]{8}$", volume.VolumeId);
    }

    [Fact]
    public void Une_base_v1_donne_un_dictionnaire_vide()
    {
        using (var connection = new SqliteConnection($"Data Source={DbPath}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            commande.CommandText = "PRAGMA user_version = 1;";
            commande.ExecuteNonQuery();
        }

        Assert.Empty(LecteurDeVolumes.Lire(DbPath));
    }
}
