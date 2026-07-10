using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Observations;

namespace InstallChecker.Identity.Access.Observations;

/// <summary>
/// La construction de l'identité d'un état d'Ω (025 § 3) : la fonction d'empreinte déclarée du
/// support — SHA-256 pour le contrat <c>user_version = 1</c> (014 § 6 raffiné : « celle-là même
/// qui produit les empreintes de contenu ») — appliquée à l'encodage à **préfixe de longueur**
/// (auto-délimitant pour tout alphabet, 025 § 2) de la suite, en ordre canonique des identifiants,
/// des couples (identifiant, empreinte de contenu). Les identifiants entrent dans l'identité :
/// deux états de mêmes contenus et d'identifiants différents sont distincts (025 § 1, P1).
/// Vit du côté du support (Access) : la fonction est celle du support, appliquée par lui —
/// jamais présumée par le moteur (025 § 2, le siège du calcul).
/// </summary>
internal static class IdentiteDeLEtatOmega
{
    public static IndexOmega Calculer(ModeleObservations modele, long versionDeContrat)
    {
        var actesTries = modele.Actes.OrderBy(a => a.Identifiant).ToList();

        var encodage = new StringBuilder();
        foreach (var acte in actesTries)
        {
            AjouterChamp(encodage, acte.Identifiant.ToString(CultureInfo.InvariantCulture));
            AjouterChamp(encodage, acte.Empreinte);
        }

        var empreinteEtat = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(encodage.ToString())));
        return new IndexOmega(versionDeContrat, actesTries.Count, empreinteEtat);
    }

    /// <summary>« n:v, » où n est la longueur de v en caractères — aucun alphabet présumé (025 § 2).</summary>
    private static void AjouterChamp(StringBuilder encodage, string valeur) =>
        encodage.Append(valeur.Length).Append(':').Append(valeur).Append(',');
}
