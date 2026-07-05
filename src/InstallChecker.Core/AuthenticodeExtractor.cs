using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace InstallChecker;

/// <summary>
/// Capacité autonome : observe le certificat feuille embarqué (Authenticode) via une ouverture indépendante.
/// Extraction seule — aucune validation de chaîne, aucune révocation, aucun accès réseau, aucun jugement.
/// </summary>
public static class AuthenticodeExtractor
{
    /// <summary>Toutes les propriétés null = aucun certificat embarqué observé (ce n'est pas une conclusion « non signé »).</summary>
    public sealed record AuthenticodeInfo(
        string? Subject, string? Issuer, string? SerialNumber, string? Thumbprint, string? NotBefore, string? NotAfter)
    {
        public static readonly AuthenticodeInfo None = new(null, null, null, null, null, null);
    }

    public static AuthenticodeInfo Read(string path)
    {
        try
        {
            // CreateFromSignedFile est marquée SYSLIB0057 en .NET 10, mais reste la seule API BCL qui extrait
            // le certificat embarqué d'un fichier signé sans le valider — X509CertificateLoader (le remplaçant
            // officiel) ne couvre pas ce cas. Chemin de sortie si l'API est retirée : P/Invoke CryptQueryObject.
#pragma warning disable SYSLIB0057
            using var signer = X509Certificate.CreateFromSignedFile(path);
#pragma warning restore SYSLIB0057
            // X509CertificateLoader (API moderne .NET 9+) pour lire les champs typés du même certificat, tel quel.
            using var cert = X509CertificateLoader.LoadCertificate(signer.GetRawCertData());
            return new AuthenticodeInfo(
                Subject: cert.Subject,
                Issuer: cert.Issuer,
                SerialNumber: cert.SerialNumber,
                Thumbprint: cert.Thumbprint,
                NotBefore: cert.NotBefore.ToString("O"),
                NotAfter: cert.NotAfter.ToString("O"));
        }
        catch (CryptographicException)
        {
            return AuthenticodeInfo.None;
        }
    }
}
