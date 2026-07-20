using System.Text.Json;

namespace InstallChecker.Tests;

public class PlanCommandTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "plan-cli-tests-" + Guid.NewGuid())).FullName;

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Directory.Delete(_root, recursive: true);
    }

    private static string RacineDuDepot()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        return repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable");
    }

    private static string CheminRegistreReel() => Path.Combine(RacineDuDepot(), "registre");

    [Fact]
    public void Le_plan_ne_propose_jamais_lexemplaire_que_le_rapport_classe_a_conserver()
    {
        File.WriteAllText(Path.Combine(_root, "setup - copy.exe"), "same-content");
        File.WriteAllText(Path.Combine(_root, "setup.exe"), "same-content");
        var cheminBase = Path.Combine(_root, "test.db");

        var codeScan = ScanCommand.Run(_root, cheminBase, jsonOutput: false, TextWriter.Null, TextWriter.Null, ["exe"]);
        Assert.Equal(0, codeScan);

        var sortieRapport = new StringWriter();
        Assert.Equal(0, DuplicatesCommand.Deriver(cheminBase, CheminRegistreReel(), sortieRapport, new StringWriter()));

        var cheminAConserver = CheminAConserver(sortieRapport.ToString());

        var sortiePlan = new StringWriter();
        Assert.Equal(0, PlanCommand.Deriver(cheminBase, CheminRegistreReel(), sortiePlan, new StringWriter()));

        using var plan = JsonDocument.Parse(sortiePlan.ToString());
        var propositions = plan.RootElement.GetProperty("Propositions").EnumerateArray().ToList();
        var cheminsProposes = propositions.Select(p => p.GetProperty("Chemin").GetString()).ToList();

        Assert.DoesNotContain(cheminAConserver, cheminsProposes);

        using var rapport = JsonDocument.Parse(sortieRapport.ToString());
        var groupe = rapport.RootElement.GetProperty("Groupes")[0];
        var proposition = Assert.Single(propositions);
        var exemplairePropose = groupe.GetProperty("Exemplaires")
            .EnumerateArray()
            .Single(e => e.GetProperty("Fichier").GetProperty("Chemin").GetString()
                       == proposition.GetProperty("Chemin").GetString());

        Assert.Equal(groupe.GetProperty("GroupeId").GetString(), proposition.GetProperty("GroupeId").GetString());
        Assert.Equal(exemplairePropose.GetProperty("FichierId").GetString(), proposition.GetProperty("FichierId").GetString());
        Assert.Equal("duplicate-files/safe-plan/v1", plan.RootElement.GetProperty("VersionContrat").GetString());
        var garantie = Assert.Single(plan.RootElement.GetProperty("GarantiesParGroupe").EnumerateArray());
        Assert.Equal(groupe.GetProperty("GroupeId").GetString(), garantie.GetProperty("GroupeId").GetString());
        Assert.Equal(
            cheminAConserver,
            garantie.GetProperty("TemoinConservation").GetProperty("Chemin").GetString());
    }

    private static string CheminAConserver(string jsonRapport)
    {
        using var rapport = JsonDocument.Parse(jsonRapport);
        return rapport.RootElement.GetProperty("Groupes")[0]
            .GetProperty("Exemplaires")
            .EnumerateArray()
            .Single(e => e.GetProperty("Etiquette").GetString() == "à conserver")
            .GetProperty("Fichier")
            .GetProperty("Chemin")
            .GetString()!;
    }
}
