using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace InstallChecker.DuplicateFiles;

/// <summary>Identifiants déterministes du contrat Duplicate Files, indépendants des ActeId.</summary>
public static class IdentifiantsStables
{
    public static string NormaliserSha256(string sha256)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);

        if (sha256.Length != 64)
            throw new ArgumentException("empreinte SHA-256 invalide", nameof(sha256));

        try
        {
            _ = Convert.FromHexString(sha256);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("empreinte SHA-256 invalide", nameof(sha256), ex);
        }

        return sha256.ToLowerInvariant();
    }

    public static string PourGroupeExact(string sha256) =>
        $"exact:sha256:{NormaliserSha256(sha256)}";

    public static string NormaliserCheminWindows(string chemin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chemin);
        return chemin.Replace('/', '\\').ToUpperInvariant();
    }

    public static string PourFichier(string sha256, string chemin)
    {
        var groupeId = PourGroupeExact(sha256);
        var cheminCanonique = NormaliserCheminWindows(chemin);
        var charge = Encoding.UTF8.GetBytes($"{groupeId}\n{cheminCanonique}");
        var empreinte = Convert.ToHexString(SHA256.HashData(charge)).ToLowerInvariant();
        return $"file:sha256:{empreinte}";
    }

    public static string PourGroupeVersionne(
        string sourceFamille,
        string cleFamille,
        SchemaVersionComparable schemaVersion,
        string format,
        string? architecture,
        string? langue,
        string? edition,
        string? distribution)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFamille);
        ArgumentException.ThrowIfNullOrWhiteSpace(cleFamille);
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        using var charge = new MemoryStream();
        foreach (var valeur in new string?[]
        {
            "version-family/v1",
            sourceFamille,
            cleFamille,
            schemaVersion.ToString(),
            format,
            architecture,
            langue,
            edition,
            distribution,
        })
        {
            EcrireChamp(charge, valeur);
        }

        var empreinte = Convert.ToHexString(SHA256.HashData(charge.ToArray())).ToLowerInvariant();
        return $"version:sha256:{empreinte}";
    }

    private static void EcrireChamp(Stream destination, string? valeur)
    {
        Span<byte> longueur = stackalloc byte[sizeof(uint)];
        if (valeur is null)
        {
            BinaryPrimitives.WriteUInt32BigEndian(longueur, uint.MaxValue);
            destination.Write(longueur);
            return;
        }

        var octets = Encoding.UTF8.GetBytes(valeur);
        BinaryPrimitives.WriteUInt32BigEndian(longueur, checked((uint)octets.Length));
        destination.Write(longueur);
        destination.Write(octets);
    }
}
