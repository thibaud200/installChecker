using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace InstallChecker;

public sealed record SnapshotCalcule(string Cle, byte[] ChargeCanonique);

/// <summary>Construit l'identité physique d'un ensemble immuable d'observations brutes.</summary>
public static class CleSnapshotObservation
{
    public const string VersionContrat = "scanner-observation/v1";

    public static SnapshotCalcule Calculer(FileObservation observation)
    {
        using var charge = new MemoryStream();

        Ecrire(charge, VersionContrat);
        Ecrire(charge, observation.Size);
        Ecrire(charge, observation.Sha256);
        Ecrire(charge, observation.MagicHex);
        Ecrire(charge, observation.Container);

        Ecrire(charge, observation.VersionInfo.ProductName);
        Ecrire(charge, observation.VersionInfo.CompanyName);
        Ecrire(charge, observation.VersionInfo.ProductVersion);
        Ecrire(charge, observation.VersionInfo.FileVersion);

        Ecrire(charge, observation.PeInfo.Machine);
        Ecrire(charge, observation.PeInfo.Subsystem);
        Ecrire(charge, observation.PeInfo.Characteristics);
        Ecrire(charge, observation.PeInfo.Timestamp);
        Ecrire(charge, observation.PeInfo.OptionalHeaderMagic);

        Ecrire(charge, observation.Authenticode.Subject);
        Ecrire(charge, observation.Authenticode.Issuer);
        Ecrire(charge, observation.Authenticode.SerialNumber);
        Ecrire(charge, observation.Authenticode.Thumbprint);
        Ecrire(charge, observation.Authenticode.NotBefore);
        Ecrire(charge, observation.Authenticode.NotAfter);

        Ecrire(charge, observation.MsiProperties.ProductName);
        Ecrire(charge, observation.MsiProperties.ProductVersion);
        Ecrire(charge, observation.MsiProperties.Manufacturer);
        Ecrire(charge, observation.MsiProperties.ProductCode);
        Ecrire(charge, observation.MsiProperties.UpgradeCode);
        Ecrire(charge, observation.MsiProperties.ProductLanguage);

        Ecrire(charge, observation.AppxManifest.Name);
        Ecrire(charge, observation.AppxManifest.Publisher);
        Ecrire(charge, observation.AppxManifest.Version);
        Ecrire(charge, observation.AppxManifest.ProcessorArchitecture);

        var octets = charge.ToArray();
        var empreinte = Convert.ToHexStringLower(SHA256.HashData(octets));
        return new SnapshotCalcule($"snapshot:sha256:{empreinte}", octets);
    }

    private static void Ecrire(Stream destination, string? valeur)
    {
        Span<byte> longueur = stackalloc byte[sizeof(int)];
        if (valeur is null)
        {
            BinaryPrimitives.WriteInt32BigEndian(longueur, -1);
            destination.Write(longueur);
            return;
        }

        var octets = Encoding.UTF8.GetBytes(valeur);
        BinaryPrimitives.WriteInt32BigEndian(longueur, octets.Length);
        destination.Write(longueur);
        destination.Write(octets);
    }

    private static void Ecrire(Stream destination, long valeur)
    {
        Span<byte> octets = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(octets, valeur);
        destination.Write(octets);
    }

    private static void Ecrire(Stream destination, long? valeur)
    {
        destination.WriteByte(valeur.HasValue ? (byte)1 : (byte)0);
        if (valeur.HasValue)
            Ecrire(destination, valeur.Value);
    }
}
