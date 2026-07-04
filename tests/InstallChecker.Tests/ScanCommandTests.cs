using InstallChecker;

namespace InstallChecker.Tests;

public class ScanCommandTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("installchecker-test-").FullName;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Scan_ListsAllFilesRecursively_WithPathAndSize()
    {
        File.WriteAllBytes(Path.Combine(_root, "a.exe"), new byte[3]);
        var sub = Directory.CreateDirectory(Path.Combine(_root, "sub"));
        File.WriteAllBytes(Path.Combine(sub.FullName, "b.txt"), new byte[7]);

        var output = new StringWriter();
        var exitCode = ScanCommand.Run(_root, output, TextWriter.Null);

        Assert.Equal(0, exitCode);
        var lines = output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
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

        var output = new StringWriter();
        var exitCode = ScanCommand.Run(_root, output, TextWriter.Null);

        Assert.Equal(0, exitCode);
        Assert.Contains($"{hidden}\t1\t", output.ToString());
    }

    // Vecteur SHA-256 connu : "abc" (FIPS 180-2, oracle indépendant de l'implémentation).
    [Fact]
    public void Scan_KnownContent_ProducesKnownSha256()
    {
        File.WriteAllText(Path.Combine(_root, "abc.txt"), "abc");

        var output = new StringWriter();
        Assert.Equal(0, ScanCommand.Run(_root, output, TextWriter.Null));
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
            HashOf(output, "abc.txt"));
    }

    [Fact]
    public void Scan_EmptyFile_ProducesEmptyContentSha256()
    {
        File.WriteAllBytes(Path.Combine(_root, "vide.bin"), []);

        var output = new StringWriter();
        Assert.Equal(0, ScanCommand.Run(_root, output, TextWriter.Null));
        Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
            HashOf(output, "vide.bin"));
    }

    [Fact]
    public void Scan_IdenticalFiles_ProduceSameHash()
    {
        File.WriteAllText(Path.Combine(_root, "un.bin"), "même contenu");
        File.WriteAllText(Path.Combine(_root, "deux.bin"), "même contenu");

        var output = new StringWriter();
        Assert.Equal(0, ScanCommand.Run(_root, output, TextWriter.Null));
        Assert.Equal(HashOf(output, "un.bin"), HashOf(output, "deux.bin"));
    }

    [Fact]
    public void Scan_DifferentFiles_ProduceDifferentHashes()
    {
        File.WriteAllText(Path.Combine(_root, "un.bin"), "contenu A");
        File.WriteAllText(Path.Combine(_root, "deux.bin"), "contenu B");

        var output = new StringWriter();
        Assert.Equal(0, ScanCommand.Run(_root, output, TextWriter.Null));
        Assert.NotEqual(HashOf(output, "un.bin"), HashOf(output, "deux.bin"));
    }

    [Fact]
    public void Scan_UnreadableFile_DoesNotStopScan()
    {
        File.WriteAllText(Path.Combine(_root, "lisible.txt"), "ok");
        var lockedPath = Path.Combine(_root, "verrouille.bin");
        using var locked = new FileStream(lockedPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var output = new StringWriter();
        var errors = new StringWriter();
        var exitCode = ScanCommand.Run(_root, output, errors);

        Assert.Equal(0, exitCode);
        Assert.Contains($"{Path.Combine(_root, "lisible.txt")}\t", output.ToString());
        Assert.DoesNotContain("verrouille.bin\t", output.ToString());
        Assert.Contains("verrouille.bin", errors.ToString());
        Assert.Contains("1 erreur(s) locale(s)", errors.ToString());
    }

    /// <summary>Extrait la colonne sha256 de la ligne du fichier nommé.</summary>
    private static string HashOf(StringWriter output, string fileName)
    {
        var line = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Single(l => l.Split('\t')[0].EndsWith(fileName));
        return line.Split('\t')[2];
    }

    [Fact]
    public void Scan_EmptyDirectory_CompletesWithZero()
    {
        var output = new StringWriter();
        Assert.Equal(0, ScanCommand.Run(_root, output, TextWriter.Null));
        Assert.Empty(output.ToString());
    }

    [Fact]
    public void Scan_MissingRoot_ReturnsOne()
    {
        var errors = new StringWriter();
        var exitCode = ScanCommand.Run(Path.Combine(_root, "n-existe-pas"), TextWriter.Null, errors);

        Assert.Equal(1, exitCode);
        Assert.Contains("introuvable", errors.ToString());
    }
}
