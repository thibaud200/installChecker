using System.Text.RegularExpressions;

namespace InstallChecker.DuplicateFiles;

public static class NormalisationVersionnee
{
    private static readonly string[] ExtensionsComposees = [".tar.gz", ".tar.bz2", ".tar.xz"];

    public static string Texte(string valeur)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valeur);
        return Regex.Replace(valeur.Trim(), @"\s+", " ", RegexOptions.CultureInvariant)
            .ToUpperInvariant();
    }

    public static string? Architecture(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
            return null;

        return Texte(valeur) switch
        {
            "X86" or "WIN32" or "I386" or "I486" or "I586" or "IA32" or "014C" => "x86",
            "X64" or "AMD64" or "WIN64" or "X86_64" or "8664" => "x64",
            "ARM" or "ARMNT" or "THUMB" or "01C0" or "01C2" or "01C4" => "arm",
            "ARM64" or "AARCH64" or "AA64" => "arm64",
            var autre => autre,
        };
    }

    public static string Format(string chemin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chemin);
        var nom = Path.GetFileName(chemin);
        foreach (var extension in ExtensionsComposees)
        {
            if (nom.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                return extension;
        }

        var extensionSimple = Path.GetExtension(nom);
        return string.IsNullOrEmpty(extensionSimple)
            ? "<sans-extension>"
            : extensionSimple.ToLowerInvariant();
    }
}
