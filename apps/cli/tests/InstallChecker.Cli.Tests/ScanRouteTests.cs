using System.Diagnostics;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

public sealed class ScanRouteTests : IDisposable
{
    private readonly string _racine = Directory.CreateTempSubdirectory("cli-scan-source-").FullName;
    private readonly string _sortie = Directory.CreateTempSubdirectory("cli-scan-output-").FullName;
    private string BasePath => Path.Combine(_sortie, "observations.db");
    private string JsonPath => Path.Combine(_sortie, "scan.jsonl");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_racine, recursive: true);
        Directory.Delete(_sortie, recursive: true);
    }

    [Fact]
    public async Task JsonFile_cree_un_jsonl_sans_json_sur_stdout_et_le_remplace()
    {
        File.WriteAllText(Path.Combine(_racine, "a.txt"), "x");

        var premier = await Executer("scan", _racine, "--db", BasePath, "--json-file", JsonPath);
        Assert.Equal(0, premier.Code);
        Assert.True(string.IsNullOrWhiteSpace(premier.Stdout));
        using (JsonDocument.Parse(Assert.Single(File.ReadAllLines(JsonPath)))) { }

        File.WriteAllText(JsonPath, "ligne parasite");
        var second = await Executer("scan", _racine, "--db", BasePath, "--json-file", JsonPath);

        Assert.Equal(0, second.Code);
        var ligne = Assert.Single(File.ReadAllLines(JsonPath));
        Assert.DoesNotContain("ligne parasite", ligne);
        using (JsonDocument.Parse(ligne)) { }
    }

    [Fact]
    public async Task Json_et_JsonFile_ensemble_retournent_une_erreur_dusage()
    {
        var resultat = await Executer("scan", _racine, "--json", "--json-file", JsonPath);

        Assert.Equal(2, resultat.Code);
        Assert.Contains("Usage", resultat.Stderr);
    }

    [Fact]
    public async Task JsonFile_sans_valeur_retourne_une_erreur_dusage()
    {
        var resultat = await Executer("scan", _racine, "--json-file");

        Assert.Equal(2, resultat.Code);
        Assert.Contains("Usage", resultat.Stderr);
    }

    private static async Task<(int Code, string Stdout, string Stderr)> Executer(params string[] arguments)
    {
        var demarrage = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        demarrage.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "InstallChecker.dll"));
        foreach (var argument in arguments)
            demarrage.ArgumentList.Add(argument);

        using var processus = Process.Start(demarrage) ?? throw new InvalidOperationException("CLI non démarrée");
        var stdout = processus.StandardOutput.ReadToEndAsync();
        var stderr = processus.StandardError.ReadToEndAsync();
        await processus.WaitForExitAsync();
        return (processus.ExitCode, await stdout, await stderr);
    }
}
