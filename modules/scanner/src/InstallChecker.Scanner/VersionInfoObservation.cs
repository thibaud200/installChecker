using System.Diagnostics;

namespace InstallChecker;

/// <summary>Valeurs brutes observées par FileVersionInfo, sans conserver de dépendance au type BCL.</summary>
public sealed record VersionInfoObservation(
    string? ProductName,
    string? CompanyName,
    string? ProductVersion,
    string? FileVersion)
{
    public static VersionInfoObservation From(FileVersionInfo source) =>
        new(source.ProductName, source.CompanyName, source.ProductVersion, source.FileVersion);
}
