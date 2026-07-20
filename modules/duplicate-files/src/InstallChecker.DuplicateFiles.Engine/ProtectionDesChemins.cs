namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Politique de protection minimale des emplacements système : un plan peut les mentionner comme
/// survivants, mais jamais proposer une suppression dedans. La comparaison se fait par racine de
/// chemin, insensible à la casse Windows.
/// </summary>
public static class ProtectionDesChemins
{
    private static readonly string[] RacinesProtegees =
    [
        @"C:\Windows",
        @"C:\Program Files",
        @"C:\Program Files (x86)",
        @"C:\$Recycle.Bin",
    ];

    public static bool EstProtegeParDefaut(string chemin)
    {
        var normalise = Normaliser(chemin);
        return RacinesProtegees.Any(racine =>
            normalise.Equals(Normaliser(racine), StringComparison.OrdinalIgnoreCase)
            || normalise.StartsWith(Normaliser(racine) + "\\", StringComparison.OrdinalIgnoreCase));
    }

    private static string Normaliser(string chemin) =>
        Path.GetFullPath(chemin).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
