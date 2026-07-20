using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

public class RedondanceVersionneeCommandTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "version-redundancy-tests-" + Guid.NewGuid())).FullName;

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void Une_base_scanee_produit_le_contrat_versionne_sans_action_destructive()
    {
        var baseSqlite = CreerBaseDeuxVersions();
        var sortie = new StringWriter();
        var erreurs = new StringWriter();

        var code = RedondanceVersionneeCommand.Deriver(baseSqlite, sortie, erreurs);

        Assert.Equal(0, code);
        Assert.Empty(erreurs.ToString());
        using var json = JsonDocument.Parse(sortie.ToString());
        Assert.Equal(
            "duplicate-files/version-redundancy/v1",
            json.RootElement.GetProperty("VersionContrat").GetString());
        Assert.Single(json.RootElement.GetProperty("Groupes").EnumerateArray());
        Assert.DoesNotContain("Supprimer", sortie.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Corbeille", sortie.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Deux_emissions_sont_identiques()
    {
        var baseSqlite = CreerBaseDeuxVersions();
        var premiere = new StringWriter();
        var seconde = new StringWriter();

        Assert.Equal(0, RedondanceVersionneeCommand.Deriver(baseSqlite, premiere, new StringWriter()));
        Assert.Equal(0, RedondanceVersionneeCommand.Deriver(baseSqlite, seconde, new StringWriter()));

        Assert.Equal(premiere.ToString(), seconde.ToString());
    }

    [Fact]
    public void Une_base_absente_ecrit_seulement_lerreur_contractuelle()
    {
        var sortie = new StringWriter();
        var erreurs = new StringWriter();

        var code = RedondanceVersionneeCommand.Deriver(
            Path.Combine(_root, "absente.db"), sortie, erreurs);

        Assert.Equal(1, code);
        Assert.Empty(sortie.ToString());
        Assert.False(string.IsNullOrWhiteSpace(erreurs.ToString()));
    }

    private string CreerBaseDeuxVersions()
    {
        File.WriteAllText(Path.Combine(_root, "outil-1.zip"), "version un");
        File.WriteAllText(Path.Combine(_root, "outil-2.zip"), "version deux");
        var baseSqlite = Path.Combine(_root, "observations.db");
        Assert.Equal(0, ScanCommand.Run(
            _root,
            baseSqlite,
            jsonOutput: false,
            TextWriter.Null,
            TextWriter.Null,
            ["zip"]));
        return baseSqlite;
    }
}
