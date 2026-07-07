namespace InstallChecker.Identity.Observations;

/// <summary>Le modèle d'observations produit par C1 (014 C1) — les actes, en ordre canonique.</summary>
public sealed record ModeleObservations(IReadOnlyList<ActeObservation> Actes);
