namespace InstallChecker.Identity.Actes;

/// <summary>L'ensemble des actes produit par C5 (012 § 1.2, 014 C5) : élections et refus, chacun couvrant un domaine-strate à espace d'hypothèses non trivial.</summary>
public sealed record EnsembleDesActes(
    IReadOnlyList<ActeElection> Elections,
    IReadOnlyList<Refus> Refus);
