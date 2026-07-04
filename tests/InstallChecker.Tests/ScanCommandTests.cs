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
        Assert.Contains($"{Path.Combine(_root, "a.exe")}\t3", lines);
        Assert.Contains($"{Path.Combine(sub.FullName, "b.txt")}\t7", lines);
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
        Assert.Contains($"{hidden}\t1", output.ToString());
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
