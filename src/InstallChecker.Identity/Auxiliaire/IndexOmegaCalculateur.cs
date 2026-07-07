using System.Security.Cryptography;
using System.Text;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Observations;

namespace InstallChecker.Identity.Auxiliaire;

// ponytail: cette fonction n'appartient à aucune des sept responsabilités C1→C7 (012 § 1.2) — ce
// n'est pas C6, qui reçoit l'index déjà constitué (014 C6 : « reçoit ... + l'index (identité de
// l'état d'Ω, identité de l'état de ℛ) »), jamais un modèle d'observations brut. C'est un utilitaire
// externe à la machine abstraite, appelé par l'orchestrateur (tests, futur appelant) pour préparer
// l'entrée que C6 exige déjà toute faite. Placé hors du dossier/namespace Etat pour qu'aucune lecture
// ne puisse l'attribuer à C6.
/// <summary>
/// Calcule <see cref="IndexOmega"/> depuis le modèle d'observations projeté par C1. La version de
/// contrat est celle du pipeline figé (014 § 6, 013 § 1.1) : il n'en existe à ce jour aucune autre.
/// L'empreinte d'état réutilise la fonction d'empreinte du support (SHA-256, déjà appliquée aux
/// empreintes de contenu individuelles) sur leur concaténation en ordre canonique d'identifiants
/// (014 § 7.2) — aucun fichier relu, aucune empreinte de contenu recalculée, aucune fonction nouvelle.
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
