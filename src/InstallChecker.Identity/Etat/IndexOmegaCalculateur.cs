using System.Security.Cryptography;
using System.Text;
using InstallChecker.Identity.Observations;

namespace InstallChecker.Identity.Etat;

/// <summary>
/// Calcule <see cref="IndexOmega"/> depuis le modèle d'observations projeté par C1 — préalable à
/// C6, jamais C6 lui-même (014 C6 : « reçoit ... + l'index », déjà constitué ; C6 ne lit jamais Ω).
/// La version de contrat supportée est celle du pipeline figé (014 § 6, 013 § 1.1) : il n'en existe
/// à ce jour aucune autre.
/// </summary>
public static class IndexOmegaCalculateur
{
    private const long VersionDeContrat = 1;

    public static IndexOmega Calculer(ModeleObservations modele)
    {
        var actesTries = modele.Actes.OrderBy(a => a.Identifiant).ToList();
        var concatenation = string.Concat(actesTries.Select(a => a.Empreinte));
        var empreinte = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(concatenation)));

        return new IndexOmega(VersionDeContrat, actesTries.Count, empreinte);
    }
}
