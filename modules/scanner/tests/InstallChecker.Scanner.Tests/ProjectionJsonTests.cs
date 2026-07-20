using InstallChecker;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

public sealed class ProjectionJsonTests : IDisposable
{
    private readonly string _racine = Directory.CreateTempSubdirectory("scanner-json-source-").FullName;
    private readonly string _sortie = Directory.CreateTempSubdirectory("scanner-json-output-").FullName;
    private string BasePath => Path.Combine(_sortie, "observations.db");
    private string JsonPath => Path.Combine(_sortie, "scan.jsonl");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_racine, recursive: true);
        Directory.Delete(_sortie, recursive: true);
    }

    [Fact]
    public void Json_du_scan_ne_peut_pas_etre_projete_avant_commit()
    {
        using var store = new ObservationStore(
            BasePath,
            new ScanDeclaration("volume", null, _racine, "2026-07-19T10:00:00Z", null));
        store.Persist(ObservationSimple(Path.Combine(_racine, "a.bin")));

        Assert.Throws<InvalidOperationException>(() => store.ProjeterJsonDuScan().ToList());
    }

    [Fact]
    public void JsonFile_remplace_le_fichier_existant()
    {
        File.WriteAllText(JsonPath, "ancienne ligne");
        File.WriteAllText(Path.Combine(_racine, "a.txt"), "x");

        var code = ScannerVersFichier();

        Assert.Equal(0, code);
        var ligne = Assert.Single(File.ReadAllLines(JsonPath));
        Assert.DoesNotContain("ancienne ligne", ligne);
        using var _ = System.Text.Json.JsonDocument.Parse(ligne);
    }

    [Fact]
    public void Deux_exports_successifs_contiennent_seulement_le_dernier_scan()
    {
        File.WriteAllText(Path.Combine(_racine, "a.txt"), "x");
        File.WriteAllText(Path.Combine(_racine, "b.txt"), "y");

        Assert.Equal(0, ScannerVersFichier());
        Assert.Equal(0, ScannerVersFichier());

        Assert.Equal(2, File.ReadAllLines(JsonPath).Length);
    }

    [Fact]
    public void Sortie_json_et_fichier_json_sont_mutuellement_exclusifs()
    {
        Assert.Throws<ArgumentException>(() =>
            ScanCommand.Run(_racine, BasePath, true, TextWriter.Null, TextWriter.Null, jsonFilePath: JsonPath));
    }

    private int ScannerVersFichier() =>
        ScanCommand.Run(
            _racine,
            BasePath,
            jsonOutput: false,
            TextWriter.Null,
            TextWriter.Null,
            jsonFilePath: JsonPath);

    private static FileObservation ObservationSimple(string path) =>
        new(
            path,
            1,
            new string('a', 64),
            "2026-07-19T10:00:00Z",
            "00",
            null,
            new VersionInfoObservation(null, null, null, null),
            PeInfoExtractor.PeInfo.None,
            AuthenticodeExtractor.AuthenticodeInfo.None,
            MsiPropertiesExtractor.MsiProperties.None,
            AppxManifestExtractor.AppxManifest.None);
}
