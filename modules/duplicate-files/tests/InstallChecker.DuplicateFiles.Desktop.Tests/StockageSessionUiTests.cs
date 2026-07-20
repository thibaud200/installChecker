using System.Text.Json;
using InstallChecker.DuplicateFiles.Desktop.Session;

namespace InstallChecker.DuplicateFiles.Desktop.Tests;

public sealed class StockageSessionUiTests : IDisposable
{
    private readonly string _repertoire = Path.Combine(
        Path.GetTempPath(),
        $"installchecker-desktop-tests-{Guid.NewGuid():N}");
    private readonly StockageSessionUi _stockage = new();

    private string Courant => Path.Combine(_repertoire, "bibliotheque.session.json");
    private string Archive => StockageSessionUi.CheminArchive(Courant);

    [Fact]
    public async Task Sauvegarder_puis_charger_preserve_rapports_et_decisions()
    {
        var session = SessionFixture("bibliotheque", "groupe-1", EtatRevueUi.Prevoir);

        await _stockage.SauvegarderAsync(Courant, session, false, default);
        var relue = await _stockage.ChargerAsync(Courant, default);

        Assert.Equal(VersionsContratUi.SessionV1, relue.VersionContrat);
        Assert.Equal(EtatRevueUi.Prevoir, relue.Decisions["groupe-1"].Etat);
        Assert.Equal(1, relue.RapportDoublons!.Value.GetProperty("Groupes").GetArrayLength());
    }

    [Fact]
    public async Task Rescan_conserve_exactement_une_archive()
    {
        await _stockage.SauvegarderAsync(Courant, SessionFixture("v1"), false, default);
        await _stockage.SauvegarderAsync(Courant, SessionFixture("v2"), true, default);
        await _stockage.SauvegarderAsync(Courant, SessionFixture("v3"), true, default);

        Assert.Equal("v3", (await _stockage.ChargerAsync(Courant, default)).Bibliotheque.Nom);
        Assert.Equal("v2", (await _stockage.ChargerAsync(Archive, default)).Bibliotheque.Nom);
        Assert.Equal(2, Directory.GetFiles(_repertoire, "*.json").Length);
    }

    [Fact]
    public async Task Echec_avant_remplacement_conserve_la_session_courante()
    {
        await _stockage.SauvegarderAsync(Courant, SessionFixture("stable"), false, default);
        var stockageEnEchec = new StockageSessionUi((_, _, _) =>
            throw new IOException("simulation"));

        await Assert.ThrowsAsync<IOException>(() =>
            stockageEnEchec.SauvegarderAsync(Courant, SessionFixture("nouvelle"), true, default));

        Assert.Equal(
            "stable",
            (await _stockage.ChargerAsync(Courant, default)).Bibliotheque.Nom);
    }

    [Fact]
    public async Task Charger_refuse_une_version_de_contrat_inconnue()
    {
        Directory.CreateDirectory(_repertoire);
        await File.WriteAllTextAsync(Courant, "{\"VersionContrat\":\"inconnue\"}");

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            _stockage.ChargerAsync(Courant, default));
    }

    public void Dispose()
    {
        if (Directory.Exists(_repertoire))
            Directory.Delete(_repertoire, recursive: true);
    }

    private SessionDuplicateFilesUi SessionFixture(
        string nom,
        string groupeId = "groupe",
        EtatRevueUi etat = EtatRevueUi.AExaminer)
    {
        using var rapport = JsonDocument.Parse("{\"Groupes\":[{\"GroupeId\":\"groupe\"}]}");
        var bibliotheque = new BibliothequeUi(
            nom,
            Path.Combine(_repertoire, "bibliotheque.db"),
            "registre",
            [@"C:\Corpus"],
            Courant);

        return new SessionDuplicateFilesUi(
            VersionsContratUi.SessionV1,
            bibliotheque,
            DateTimeOffset.Parse("2026-07-20T12:00:00+02:00"),
            rapport.RootElement.Clone(),
            null,
            new Dictionary<string, DecisionRevueUi>
            {
                [groupeId] = new(groupeId, null, etat, DateTimeOffset.UtcNow)
            },
            new EtatFiltresUi(string.Empty, null, null),
            new EtatFiltresUi(string.Empty, null, null),
            []);
    }
}
