using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.Identity.Actes;

/// <summary>
/// Un acte d'élection e = (h, 𝒮, (Ω, K), niveau, motif) — 006, Déf. 1. Confère à une hypothèse le
/// statut « retenue » sans en changer les constituants (I21) : la hypothèse retenue n'est pas
/// dupliquée ici (elle reste dérivable depuis C4) — l'acte porte le contenu propositionnel, le
/// domaine, le niveau assigné, le motif, les licences qui l'autorisent (I27 : jamais vide) et les
/// dépendances complètes (dette identifiée en sous-ensemble, toujours vide sous ℛ₀ : aucune
/// convention de priorité n'existe encore pour créer un arbitrage).
/// </summary>
public sealed record ActeElection(
    Strate Strate,
    IReadOnlyList<long> Domaine,
    string ContenuPropositionnel,
    Niveau Niveau,
    string Motif,
    IReadOnlyList<ConventionRef> Licences,
    IReadOnlyList<ConventionRef> Dependances,
    IReadOnlyList<ConventionRef> Dette);
