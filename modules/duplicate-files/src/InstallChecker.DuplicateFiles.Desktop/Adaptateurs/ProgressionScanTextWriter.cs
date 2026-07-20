using System.IO;
using System.Text;

namespace InstallChecker.DuplicateFiles.Desktop.Adaptateurs;

public sealed record ProgressionScanUi(long FichiersTraites, string CheminCourant);

public sealed class ProgressionScanTextWriter(Action<ProgressionScanUi> publier) : TextWriter
{
    private long _fichiersTraites;

    public override Encoding Encoding => Encoding.UTF8;

    public long FichiersTraites => Interlocked.Read(ref _fichiersTraites);

    public override void WriteLine(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var tabulation = value.IndexOf('\t');
        if (tabulation <= 0)
            return;

        var chemin = value[..tabulation];
        var nombre = Interlocked.Increment(ref _fichiersTraites);
        publier(new ProgressionScanUi(nombre, chemin));
    }
}
