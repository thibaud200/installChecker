namespace InstallChecker.Identity.Signaux;

/// <summary>
/// Une instance de signal s = σ(O) (002 Déf. 2) : sortie, régime porté — jamais jugé — et provenance
/// complète : la liste exacte des observations élémentaires consommées (I7, I30).
/// </summary>
public sealed record InstanceDeSignal(
    TypeDeSignal Type,
    string Sortie,
    Regime Regime,
    IReadOnlyList<ObservationConsommee> Provenance);
