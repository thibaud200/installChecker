using System.Runtime.InteropServices;
using System.Text;

namespace InstallChecker;

/// <summary>
/// L'identité observée du volume portant une racine de scan (spec multi-disque D3) : ce qui permet
/// au « dernier scan par volume » de reconnaître le même disque physique quand la lettre change.
/// Métadonnée du système de fichiers uniquement (garde A1) — jamais de contenu.
/// </summary>
public sealed record VolumeIdentity(string VolumeId, string? VolumeLabel);

public static class VolumeIdentityExtractor
{
    /// <summary>
    /// Résout l'identité du volume de <paramref name="root"/> : racine UNC normalisée en minuscules
    /// pour le réseau (lettre mappée résolue en UNC — le même partage via Z: ou via UNC est le même
    /// volume), numéro de série hexadécimal pour un disque local. Identité irrésoluble = erreur
    /// explicite : un volume mal identifié corromprait le remplacement de l'état courant.
    /// </summary>
    public static VolumeIdentity Resolve(string root)
    {
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(root);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {root} : {ex.Message}");
        }

        if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
            return new VolumeIdentity(NormalizeUncRoot(fullPath), null);

        var driveRoot = Path.GetPathRoot(fullPath);
        if (driveRoot is null || driveRoot.Length < 3 || fullPath[1] != ':')
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {root}");

        if (new DriveInfo(driveRoot).DriveType == DriveType.Network)
            return new VolumeIdentity(NormalizeUncRoot(ResolveMappedDrive(driveRoot[..2])), null);

        return ResolveLocal(driveRoot);
    }

    private static string NormalizeUncRoot(string uncPath)
    {
        var parts = uncPath.TrimStart('\\').Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {uncPath} (racine UNC sans partage)");
        return $@"\\{parts[0]}\{parts[1]}".ToLowerInvariant();
    }

    private static string ResolveMappedDrive(string driveLetter)
    {
        var length = 1024;
        var remoteName = new StringBuilder(length);
        var result = WNetGetConnectionW(driveLetter, remoteName, ref length);
        if (result != 0)
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {driveLetter} (WNetGetConnection={result})");
        return remoteName.ToString();
    }

    private static VolumeIdentity ResolveLocal(string driveRoot)
    {
        var label = new StringBuilder(261); // MAX_PATH + 1, taille documentée pour lpVolumeNameBuffer
        if (!GetVolumeInformationW(driveRoot, label, label.Capacity, out var serial, out _, out _, null, 0))
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {driveRoot} (Win32={Marshal.GetLastWin32Error()})");
        return new VolumeIdentity(serial.ToString("x8"), label.Length == 0 ? null : label.ToString());
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetVolumeInformationW(
        string lpRootPathName, StringBuilder lpVolumeNameBuffer, int nVolumeNameSize,
        out uint lpVolumeSerialNumber, out uint lpMaximumComponentLength, out uint lpFileSystemFlags,
        StringBuilder? lpFileSystemNameBuffer, int nFileSystemNameSize);

    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetGetConnectionW(string lpLocalName, StringBuilder lpRemoteName, ref int lpnLength);
}
