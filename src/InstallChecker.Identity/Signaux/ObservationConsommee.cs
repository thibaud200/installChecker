namespace InstallChecker.Identity.Signaux;

/// <summary>Une observation élémentaire consommée par une instance de signal : l'acte et l'attribut (002 § 1.1, provenance).</summary>
public sealed record ObservationConsommee(long ActeId, string Attribut);
