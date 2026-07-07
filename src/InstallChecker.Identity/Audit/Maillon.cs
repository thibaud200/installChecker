using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Audit;

/// <summary>
/// Un maillon de chaîne d'audit (012 § 6) : la couche productrice, les observations et les
/// conventions qu'elle a consommées, l'objet qu'elle a produit (décrit, jamais recalculé) et son
/// statut. Identifiable et re-dérivable isolément (I39) — c'est la seule raison d'être de ce type.
/// </summary>
public sealed record Maillon(
    Couche Couche,
    IReadOnlyList<ObservationConsommee> Observations,
    IReadOnlyList<ConventionRef> Conventions,
    string ObjetProduit,
    StatutMaillon Statut);
