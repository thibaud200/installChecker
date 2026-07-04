using System.Diagnostics;
using InstallChecker;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

public class ScanCommandTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("installchecker-scan-").FullName;
    private readonly string _dbDir = Directory.CreateTempSubdirectory("installchecker-db-").FullName;
    private string DbPath => Path.Combine(_dbDir, "test.db"); // hors du dossier scanné : la base ne doit pas apparaître dans ses propres observations

    public void Dispose()
    {
        SqliteConnection.ClearAllPools(); // libère les handles sur le fichier .db avant suppression
        Directory.Delete(_root, recursive: true);
        Directory.Delete(_dbDir, recursive: true);
    }

    private (int ExitCode, string[] Lines, string Errors) Scan()
    {
        var output = new StringWriter();
        var errors = new StringWriter();
        var exitCode = ScanCommand.Run(_root, DbPath, output, errors);
        var lines = output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        return (exitCode, lines, errors.ToString());
    }

    private List<(string Path, long Size, string Sha256)> ReadObservations()
    {
        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();
        using var select = connection.CreateCommand();
        select.CommandText = "SELECT path, size, sha256, scanned_at FROM scan_observations ORDER BY id;";
        using var reader = select.ExecuteReader();
        var rows = new List<(string, long, string)>();
        while (reader.Read())
        {
            Assert.False(reader.IsDBNull(3)); // scanned_at toujours renseigné
            rows.Add((reader.GetString(0), reader.GetInt64(1), reader.GetString(2)));
        }
        return rows;
    }

    [Fact]
    public void Scan_ListsAllFilesRecursively_WithPathAndSize()
    {
        File.WriteAllBytes(Path.Combine(_root, "a.exe"), new byte[3]);
        var sub = Directory.CreateDirectory(Path.Combine(_root, "sub"));
        File.WriteAllBytes(Path.Combine(sub.FullName, "b.txt"), new byte[7]);

        var (exitCode, lines, _) = Scan();

        Assert.Equal(0, exitCode);
        Assert.Equal(2, lines.Length);
        Assert.Contains(lines, l => l.StartsWith($"{Path.Combine(_root, "a.exe")}\t3\t"));
        Assert.Contains(lines, l => l.StartsWith($"{Path.Combine(sub.FullName, "b.txt")}\t7\t"));
        Assert.All(lines, l => Assert.Equal(64, l.Split('\t')[2].Length));
    }

    [Fact]
    public void Scan_IncludesHiddenFiles()
    {
        var hidden = Path.Combine(_root, "hidden.dat");
        File.WriteAllBytes(hidden, new byte[1]);
        File.SetAttributes(hidden, FileAttributes.Hidden);

        var (exitCode, lines, _) = Scan();

        Assert.Equal(0, exitCode);
        Assert.Contains(lines, l => l.StartsWith($"{hidden}\t1\t"));
    }

    [Fact]
    public void Scan_EmptyDirectory_CompletesWithZero()
    {
        var (exitCode, lines, _) = Scan();
        Assert.Equal(0, exitCode);
        Assert.Empty(lines);
    }

    [Fact]
    public void Scan_MissingRoot_ReturnsOne()
    {
        var errors = new StringWriter();
        var exitCode = ScanCommand.Run(Path.Combine(_root, "n-existe-pas"), DbPath, TextWriter.Null, errors);

        Assert.Equal(1, exitCode);
        Assert.Contains("introuvable", errors.ToString());
    }

    // Vecteur SHA-256 connu : "abc" (FIPS 180-2, oracle indépendant de l'implémentation).
    [Fact]
    public void Scan_KnownContent_ProducesKnownSha256()
    {
        File.WriteAllText(Path.Combine(_root, "abc.txt"), "abc");

        var (exitCode, lines, _) = Scan();

        Assert.Equal(0, exitCode);
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", HashOf(lines, "abc.txt"));
    }

    [Fact]
    public void Scan_EmptyFile_ProducesEmptyContentSha256()
    {
        File.WriteAllBytes(Path.Combine(_root, "vide.bin"), []);

        var (exitCode, lines, _) = Scan();

        Assert.Equal(0, exitCode);
        Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", HashOf(lines, "vide.bin"));
    }

    [Fact]
    public void Scan_IdenticalFiles_ProduceSameHash()
    {
        File.WriteAllText(Path.Combine(_root, "un.bin"), "même contenu");
        File.WriteAllText(Path.Combine(_root, "deux.bin"), "même contenu");

        var (_, lines, _) = Scan();

        Assert.Equal(HashOf(lines, "un.bin"), HashOf(lines, "deux.bin"));
    }

    [Fact]
    public void Scan_DifferentFiles_ProduceDifferentHashes()
    {
        File.WriteAllText(Path.Combine(_root, "un.bin"), "contenu A");
        File.WriteAllText(Path.Combine(_root, "deux.bin"), "contenu B");

        var (_, lines, _) = Scan();

        Assert.NotEqual(HashOf(lines, "un.bin"), HashOf(lines, "deux.bin"));
    }

    [Fact]
    public void Scan_CreatesDatabaseAndSchemaAutomatically()
    {
        File.WriteAllText(Path.Combine(_root, "a.txt"), "x");
        Assert.False(File.Exists(DbPath));

        var (exitCode, _, _) = Scan();

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(DbPath));
        Assert.Single(ReadObservations()); // le schéma existe et l'insertion a fonctionné
    }

    [Fact]
    public void Scan_WritesObservationsMatchingDisplayedOutput()
    {
        File.WriteAllText(Path.Combine(_root, "a.txt"), "alpha");
        File.WriteAllText(Path.Combine(_root, "b.txt"), "beta");

        var (_, lines, _) = Scan();

        var displayed = lines.Select(l => l.Split('\t')).Select(c => (c[0], long.Parse(c[1]), c[2])).ToHashSet();
        var stored = ReadObservations().ToHashSet();
        Assert.Equal(displayed, stored); // stdout et base contiennent exactement les mêmes observations
    }

    [Fact]
    public void Scan_RunTwice_AppendsObservationsWithoutDeduplication()
    {
        File.WriteAllText(Path.Combine(_root, "a.txt"), "alpha");
        File.WriteAllText(Path.Combine(_root, "b.txt"), "beta");

        Scan();
        Scan();

        var rows = ReadObservations();
        Assert.Equal(4, rows.Count); // 2 fichiers × 2 scans = 4 observations, aucune fusion
        Assert.Equal(2, rows.Count(r => r.Path == Path.Combine(_root, "a.txt")));
    }

    [Fact]
    public void Scan_UnreadableFile_DoesNotStopScanNorOtherInsertions()
    {
        File.WriteAllText(Path.Combine(_root, "lisible.txt"), "ok");
        var lockedPath = Path.Combine(_root, "verrouille.bin");
        using var locked = new FileStream(lockedPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var (exitCode, lines, errors) = Scan();

        Assert.Equal(0, exitCode);
        Assert.Contains(lines, l => l.StartsWith($"{Path.Combine(_root, "lisible.txt")}\t"));
        Assert.DoesNotContain(lines, l => l.Contains("verrouille.bin"));
        Assert.Contains("verrouille.bin", errors);
        Assert.Contains("1 erreur(s) locale(s)", errors);

        var rows = ReadObservations();
        Assert.Single(rows); // le fichier lisible est bien inséré, le fichier illisible absent
        Assert.EndsWith("lisible.txt", rows[0].Path);
    }

    [Fact]
    public void Scan_FileWithVersionInfo_StoresExactlyWhatApiReturns()
    {
        var copied = Path.Combine(_root, "kernel32.dll");
        File.Copy(Path.Combine(Environment.SystemDirectory, "kernel32.dll"), copied);
        var expected = FileVersionInfo.GetVersionInfo(copied); // oracle : l'API elle-même

        var (exitCode, _, _) = Scan();

        Assert.Equal(0, exitCode);
        var row = ReadVersionInfo(copied);
        Assert.Equal(expected.ProductName, row.ProductName);
        Assert.Equal(expected.CompanyName, row.CompanyName);
        Assert.Equal(expected.ProductVersion, row.ProductVersion);
        Assert.Equal(expected.FileVersion, row.FileVersion);
        Assert.NotNull(row.CompanyName); // kernel32 a toujours un CompanyName : le test ne valide pas du NULL contre du NULL
    }

    [Fact]
    public void Scan_FileWithoutVersionInfo_StoresNullsWithoutError()
    {
        var plain = Path.Combine(_root, "sans-version.txt");
        File.WriteAllText(plain, "aucune ressource VersionInfo ici");

        var (exitCode, _, errors) = Scan();

        Assert.Equal(0, exitCode);
        Assert.Contains("0 erreur(s) locale(s)", errors);
        var row = ReadVersionInfo(plain);
        Assert.Null(row.ProductName);
        Assert.Null(row.CompanyName);
        Assert.Null(row.ProductVersion);
        Assert.Null(row.FileVersion);
    }

    [Fact]
    public void Scan_EveryObservationHasExactlyOneVersionInfoRow()
    {
        File.WriteAllText(Path.Combine(_root, "a.txt"), "alpha");
        File.WriteAllText(Path.Combine(_root, "b.txt"), "beta");

        Scan();
        Scan();

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();
        using var count = connection.CreateCommand();
        count.CommandText = """
            SELECT (SELECT COUNT(*) FROM scan_observations),
                   (SELECT COUNT(*) FROM version_info),
                   (SELECT COUNT(DISTINCT observation_id) FROM version_info),
                   (SELECT COUNT(*) FROM file_headers),
                   (SELECT COUNT(DISTINCT observation_id) FROM file_headers);
            """;
        using var reader = count.ExecuteReader();
        reader.Read();
        Assert.Equal(4, reader.GetInt64(0));
        Assert.Equal(4, reader.GetInt64(1));
        Assert.Equal(4, reader.GetInt64(2)); // 1 ligne version_info par observation, liée par observation_id
        Assert.Equal(4, reader.GetInt64(3));
        Assert.Equal(4, reader.GetInt64(4)); // même invariant pour file_headers
    }

    [Fact]
    public void Scan_PeExecutable_ClassifiedAsPe()
    {
        var copied = Path.Combine(_root, "kernel32.dll");
        File.Copy(Path.Combine(Environment.SystemDirectory, "kernel32.dll"), copied);
        var expectedHex = ExpectedMagicHex(copied);

        Scan();

        var (magicHex, container) = ReadFileHeader(copied);
        Assert.Equal(expectedHex, magicHex);
        Assert.StartsWith("4d5a", magicHex); // "MZ"
        Assert.Equal("pe", container);
    }

    [Fact]
    public void Scan_OleCompoundFile_ClassifiedAsOleCfb()
    {
        var msi = Path.Combine(_root, "fabrique.msi");
        File.WriteAllBytes(msi, [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, 0x00, 0x42]);

        Scan();

        var (magicHex, container) = ReadFileHeader(msi);
        Assert.Equal("d0cf11e0a1b11ae1", magicHex);
        Assert.Equal("ole-cfb", container);
    }

    [Fact]
    public void Scan_ZipArchive_ClassifiedAsZip()
    {
        var zip = Path.Combine(_root, "archive.zip");
        using (var archive = System.IO.Compression.ZipFile.Open(zip, System.IO.Compression.ZipArchiveMode.Create))
            archive.CreateEntry("contenu.txt");
        var expectedHex = ExpectedMagicHex(zip);

        Scan();

        var (magicHex, container) = ReadFileHeader(zip);
        Assert.Equal(expectedHex, magicHex);
        Assert.StartsWith("504b0304", magicHex); // "PK\x03\x04"
        Assert.Equal("zip", container);
    }

    [Fact]
    public void Scan_UnknownFile_ContainerIsNull()
    {
        var texte = Path.Combine(_root, "inconnu.txt");
        File.WriteAllText(texte, "juste du texte");
        var expectedHex = ExpectedMagicHex(texte);

        Scan();

        var (magicHex, container) = ReadFileHeader(texte);
        Assert.Equal(expectedHex, magicHex);
        Assert.Equal(16, magicHex.Length); // 8 octets présents
        Assert.Null(container);
    }

    [Fact]
    public void Scan_FileShorterThanEightBytes_StoresOnlyPresentBytes()
    {
        var court = Path.Combine(_root, "court.bin");
        File.WriteAllBytes(court, [0x4D, 0x5A, 0x42]); // commence par "MZ" mais 3 octets seulement

        Scan();

        var (magicHex, container) = ReadFileHeader(court);
        Assert.Equal("4d5a42", magicHex); // exactement les octets présents, rien de plus
        Assert.Equal("pe", container);    // le motif MZ est présent : classification identique
    }

    /// <summary>Oracle : hex minuscule des 8 premiers octets réels du fichier, lus par le test lui-même.</summary>
    private static string ExpectedMagicHex(string path)
    {
        using var stream = File.OpenRead(path);
        var buffer = new byte[8];
        var read = stream.ReadAtLeast(buffer, 8, throwOnEndOfStream: false);
        return Convert.ToHexStringLower(buffer.AsSpan(..read));
    }

    private (string MagicHex, string? Container) ReadFileHeader(string path)
    {
        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();
        using var select = connection.CreateCommand();
        select.CommandText = """
            SELECT h.magic_hex, h.container
            FROM file_headers h
            JOIN scan_observations o ON o.id = h.observation_id
            WHERE o.path = $path;
            """;
        select.Parameters.AddWithValue("$path", path);
        using var reader = select.ExecuteReader();
        Assert.True(reader.Read(), $"aucune ligne file_headers pour {path}");
        var row = (reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1));
        Assert.False(reader.Read(), $"plusieurs lignes file_headers pour {path}");
        return row;
    }

    private (string? ProductName, string? CompanyName, string? ProductVersion, string? FileVersion) ReadVersionInfo(string path)
    {
        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();
        using var select = connection.CreateCommand();
        select.CommandText = """
            SELECT v.product_name, v.company_name, v.product_version, v.file_version
            FROM version_info v
            JOIN scan_observations o ON o.id = v.observation_id
            WHERE o.path = $path;
            """;
        select.Parameters.AddWithValue("$path", path);
        using var reader = select.ExecuteReader();
        Assert.True(reader.Read(), $"aucune ligne version_info pour {path}");
        string? Col(int i) => reader.IsDBNull(i) ? null : reader.GetString(i);
        var row = (Col(0), Col(1), Col(2), Col(3));
        Assert.False(reader.Read(), $"plusieurs lignes version_info pour {path}");
        return row;
    }

    /// <summary>Extrait la colonne sha256 de la ligne du fichier nommé.</summary>
    private static string HashOf(string[] lines, string fileName) =>
        lines.Single(l => l.Split('\t')[0].EndsWith(fileName)).Split('\t')[2];
}
