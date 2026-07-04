namespace InstallChecker;

/// <summary>
/// Capacité autonome : observe les 8 premiers octets d'un fichier via une ouverture indépendante.
/// Aucune extension consultée, aucune lecture au-delà de 8 octets, aucune identification d'installateur.
/// </summary>
public static class FileHeaderExtractor
{
    /// <returns>
    /// MagicHex : les octets réellement présents (moins de 8 si le fichier est plus court), en hexadécimal minuscule.
    /// Container : "pe", "ole-cfb", "zip", ou null si non reconnu — dit ce que les octets sont, pas ce que le fichier représente.
    /// </returns>
    public static (string MagicHex, string? Container) Read(string path)
    {
        using var stream = File.OpenRead(path);
        Span<byte> buffer = stackalloc byte[8];
        var bytesRead = stream.ReadAtLeast(buffer, 8, throwOnEndOfStream: false);
        var magic = buffer[..bytesRead];
        return (Convert.ToHexStringLower(magic), Classify(magic));
    }

    private static string? Classify(ReadOnlySpan<byte> magic) => magic switch
    {
        [0x4D, 0x5A, ..] => "pe",                                  // "MZ"
        [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1] => "ole-cfb",
        [0x50, 0x4B, 0x03, 0x04, ..] => "zip",                     // "PK\x03\x04"
        _ => null,
    };
}
