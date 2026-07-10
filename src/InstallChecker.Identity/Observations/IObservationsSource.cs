using InstallChecker.Identity.Etat;

namespace InstallChecker.Identity.Observations;

/// <summary>
/// Le port « source d'observations » consommé par C1 (013 § 1.1, § 5). Trois lectures, jamais un
/// seul objet les mêlant : le modèle (dérivation) et le contexte (restitution C7 exclusivement)
/// empruntent des canaux séparés par construction (014 § 3, ligne C1 → C7) ; l'identité de l'état
/// (025 § 3) est produite par le support — selon sa fonction d'empreinte déclarée (014 § 6 raffiné)
/// appliquée à l'encodage à préfixe de longueur des couples (identifiant, empreinte de contenu) —
/// et convoyée à C6 par la ligne C1 → C6 (025 § 4, la symétrie exacte de C2 → C6).
/// </summary>
public interface IObservationsSource
{
    ModeleObservations ProjeterModele();

    IReadOnlyList<ContexteObservation> ProjeterContexte();

    IndexOmega ProjeterIdentite();
}
