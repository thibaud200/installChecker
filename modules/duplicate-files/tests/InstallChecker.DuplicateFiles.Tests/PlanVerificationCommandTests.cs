using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InstallChecker.DuplicateFiles;

namespace InstallChecker.Tests;

public class PlanVerificationCommandTests : IDisposable
{
    private static readonly JsonSerializerOptions OptionsJson = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "plan-verification-tests-" + Guid.NewGuid())).FullName;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Un_plan_valide_produit_un_rapport_executable_et_le_code_zero()
    {
        var fixture = EcrirePlanValide();
        var temoinAvant = File.ReadAllBytes(fixture.Temoin);
        var candidatAvant = File.ReadAllBytes(fixture.Candidat);
        var output = new StringWriter();
        var errors = new StringWriter();

        var code = PlanVerificationCommand.Verifier(fixture.CheminPlan, output, errors);

        Assert.Equal(0, code);
        Assert.Equal(string.Empty, errors.ToString());
        using var json = JsonDocument.Parse(output.ToString());
        Assert.Equal(
            "duplicate-files/safe-plan-verification/v1",
            json.RootElement.GetProperty("VersionContrat").GetString());
        Assert.Equal("Simulation", json.RootElement.GetProperty("Mode").GetString());
        Assert.True(json.RootElement.GetProperty("Executable").GetBoolean());
        Assert.Equal(temoinAvant, File.ReadAllBytes(fixture.Temoin));
        Assert.Equal(candidatAvant, File.ReadAllBytes(fixture.Candidat));
    }

    [Fact]
    public void Un_candidat_modifie_produit_un_rapport_bloque_et_le_code_trois()
    {
        var fixture = EcrirePlanValide();
        var temoinAvant = File.ReadAllBytes(fixture.Temoin);
        File.WriteAllText(fixture.Candidat, "contenu modifie");
        var candidatModifie = File.ReadAllBytes(fixture.Candidat);
        var output = new StringWriter();

        var code = PlanVerificationCommand.Verifier(fixture.CheminPlan, output, new StringWriter());

        Assert.Equal(3, code);
        using var json = JsonDocument.Parse(output.ToString());
        Assert.False(json.RootElement.GetProperty("Executable").GetBoolean());
        Assert.Contains(
            json.RootElement.GetProperty("Journal").EnumerateArray(),
            e => e.GetProperty("Etat").GetString() == "HashDifferent");
        Assert.Equal(temoinAvant, File.ReadAllBytes(fixture.Temoin));
        Assert.Equal(candidatModifie, File.ReadAllBytes(fixture.Candidat));
    }

    [Fact]
    public void Un_plan_absent_retourne_un_sans_sortie_partielle()
    {
        var output = new StringWriter();
        var errors = new StringWriter();

        var code = PlanVerificationCommand.Verifier(Path.Combine(_root, "absent.json"), output, errors);

        Assert.Equal(1, code);
        Assert.Equal(string.Empty, output.ToString());
        Assert.NotEqual(string.Empty, errors.ToString());
    }

    [Fact]
    public void Un_json_malforme_retourne_un_sans_sortie_partielle()
    {
        var cheminPlan = Path.Combine(_root, "malforme.json");
        File.WriteAllText(cheminPlan, "{ pas du json }");
        var output = new StringWriter();
        var errors = new StringWriter();

        var code = PlanVerificationCommand.Verifier(cheminPlan, output, errors);

        Assert.Equal(1, code);
        Assert.Equal(string.Empty, output.ToString());
        Assert.NotEqual(string.Empty, errors.ToString());
    }

    [Fact]
    public void Un_identifiant_falsifie_retourne_un_avant_toute_simulation()
    {
        var fixture = EcrirePlanValide();
        var proposition = Assert.Single(fixture.Plan.Propositions) with
        {
            FichierId = "file:sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
        };
        EcrirePlan(fixture.CheminPlan, fixture.Plan with { Propositions = [proposition] });
        var output = new StringWriter();
        var errors = new StringWriter();

        var code = PlanVerificationCommand.Verifier(fixture.CheminPlan, output, errors);

        Assert.Equal(1, code);
        Assert.Equal(string.Empty, output.ToString());
        Assert.NotEqual(string.Empty, errors.ToString());
    }

    private FixturePlan EcrirePlanValide()
    {
        var temoin = Path.Combine(_root, "garde.bin");
        var candidat = Path.Combine(_root, "copie.bin");
        File.WriteAllText(temoin, "contenu identique");
        File.WriteAllText(candidat, "contenu identique");

        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(temoin))).ToLowerInvariant();
        var plan = ConstructeurDePlan.Construire(
            new[] { (hash, (IReadOnlyList<string>)[temoin, candidat]) },
            _ => false);
        var cheminPlan = Path.Combine(_root, "plan.json");
        EcrirePlan(cheminPlan, plan);
        return new FixturePlan(cheminPlan, temoin, candidat, plan);
    }

    private static void EcrirePlan(string cheminPlan, PlanDeSuppression plan) =>
        File.WriteAllText(cheminPlan, JsonSerializer.Serialize(plan, OptionsJson));

    private sealed record FixturePlan(
        string CheminPlan,
        string Temoin,
        string Candidat,
        PlanDeSuppression Plan);
}
