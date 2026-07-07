using InstallChecker.Identity.Observations;

namespace InstallChecker.Identity.Access.Observations;

/// <summary>
/// Adaptateur de test en mémoire (013 § 5, 014 § 10, É4) : implémente le même port que
/// <see cref="LecteurDObservationsSqlite"/> par simple restitution de valeurs déjà construites —
/// la preuve vivante que le moteur ne dépend que du contrat du port, jamais de SQLite (I42).
/// </summary>
public sealed class SourceObservationsEnMemoire(ModeleObservations modele, IReadOnlyList<ContexteObservation> contexte)
    : IObservationsSource
{
    public ModeleObservations ProjeterModele() => modele;

    public IReadOnlyList<ContexteObservation> ProjeterContexte() => contexte;
}
