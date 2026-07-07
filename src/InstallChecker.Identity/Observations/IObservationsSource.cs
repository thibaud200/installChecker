namespace InstallChecker.Identity.Observations;

/// <summary>
/// Le port « source d'observations » consommé par C1 (013 § 1.1, § 5). Deux méthodes distinctes,
/// jamais un seul objet mêlant les deux : le modèle (dérivation) et le contexte (restitution
/// C7 exclusivement) empruntent des canaux séparés par construction (014 § 3, ligne C1 → C7).
/// </summary>
public interface IObservationsSource
{
    ModeleObservations ProjeterModele();

    IReadOnlyList<ContexteObservation> ProjeterContexte();
}
