using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

/// <summary>
/// La chaîne complète du chantier multi-disque (spec 2026-07-16, A4) : scan v2 → lecteur Ω filtré
/// à l'état courant → rapport porteur des volumes. Le scénario à deux volumes est prouvé au niveau
/// du lecteur (LecteurDObservationsSqliteTests) — ici, la propriété de bout en bout : un rescan
/// n'introduit aucun faux doublon.
/// </summary>
public class MultiDisqueTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("installchecker-multidisque-").FullName;
    private readonly string _dbDir = Directory.CreateTempSubdirectory("installchecker-multidisque-db-").FullName;
    private string DbPath => Path.Combine(_dbDir, "test.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
        Directory.Delete(_dbDir, recursive: true);
    }

    private static string CheminRegistreReel()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        var racine = repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable");
        return Path.Combine(racine, "registre");
    }

    [Fact]
    public void Un_rescan_du_meme_volume_ne_fabrique_aucun_faux_doublon_et_le_rapport_porte_le_volume()
    {
        File.WriteAllText(Path.Combine(_root, "original.exe"), "même contenu");
        File.WriteAllText(Path.Combine(_root, "copie.exe"), "même contenu");

        // Deux scans successifs du même dossier : sans état courant, 4 observations de même
        // empreinte donneraient un groupe de 4 « doublons » dont 2 fantômes.
        Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));
        Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));

        using (var connexion = new SqliteConnection($"Data Source={DbPath}"))
        {
            connexion.Open();
            using var compte = connexion.CreateCommand();
            compte.CommandText = "SELECT COUNT(*) FROM observation_snapshots;";
            Assert.Equal(1L, compte.ExecuteScalar());
            compte.CommandText = "SELECT COUNT(*) FROM scan_entries;";
            Assert.Equal(4L, compte.ExecuteScalar());
        }

        var sortie = new StringWriter();
        var erreurs = new StringWriter();
        Assert.Equal(0, DuplicatesCommand.Deriver(DbPath, CheminRegistreReel(), sortie, erreurs));
        Assert.Empty(erreurs.ToString());

        using var json = JsonDocument.Parse(sortie.ToString());
        var groupe = Assert.Single(json.RootElement.GetProperty("Groupes").EnumerateArray());
        var exemplaires = groupe.GetProperty("Exemplaires");
        Assert.Equal(2, exemplaires.GetArrayLength()); // les 2 fichiers du dernier scan, jamais 4

        foreach (var exemplaire in exemplaires.EnumerateArray())
        {
            var fichier = exemplaire.GetProperty("Fichier");
            Assert.Matches("^[0-9a-f]{8}$", fichier.GetProperty("VolumeId").GetString());
        }
    }
}
