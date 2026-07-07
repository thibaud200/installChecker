using InstallChecker.Identity.Conventions;

namespace InstallChecker.Identity.Audit;

/// <summary>Réponse à « de quelles conventions dépend cet acte ? » (011 § 7, 014 § 9) : Dep trié, dette identifiée en son sein (004 § 10).</summary>
public sealed record DependancesReponse(IReadOnlyList<ConventionRef> Dependances, IReadOnlyList<ConventionRef> Dette);
