using System.Reflection.PortableExecutable;

namespace InstallChecker;

/// <summary>
/// Capacité autonome : observe les en-têtes PE (COFF + optionnel) via une ouverture indépendante.
/// Valeurs brutes uniquement : hexadécimal 4 chiffres pour les champs u16, entiers pour les autres.
/// Ne dépend d'aucune autre capacité ; détermine lui-même si le fichier est un PE.
/// </summary>
public static class PeInfoExtractor
{
    /// <summary>Toutes les propriétés null = le fichier n'est pas un PE lisible (pas une erreur).</summary>
    public sealed record PeInfo(string? Machine, string? Subsystem, long? Characteristics, long? Timestamp, string? OptionalHeaderMagic)
    {
        public static readonly PeInfo None = new(null, null, null, null, null);
    }

    public static PeInfo Read(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream); // BCL (System.Reflection.Metadata) : lit uniquement les en-têtes ici
            var coff = pe.PEHeaders.CoffHeader;
            var optional = pe.PEHeaders.PEHeader; // absent sur les objets COFF purs
            return new PeInfo(
                Machine: $"{(ushort)coff.Machine:x4}",
                Subsystem: optional is null ? null : $"{(ushort)optional.Subsystem:x4}",
                Characteristics: (ushort)coff.Characteristics,
                Timestamp: (uint)coff.TimeDateStamp,
                OptionalHeaderMagic: optional is null ? null : $"{(ushort)optional.Magic:x4}");
        }
        // ArgumentException : le constructeur PEReader refuse tout flux dont la taille excède
        // int.MaxValue (« Stream length ... too large to hold a PEImage ») — bug A1 de la
        // campagne corpus 1. Même signification observable qu'un non-PE : observation toute NULL.
        catch (Exception ex) when (ex is BadImageFormatException or ArgumentException)
        {
            return PeInfo.None;
        }
    }
}
