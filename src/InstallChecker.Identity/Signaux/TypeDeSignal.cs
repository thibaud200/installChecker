using InstallChecker.Identity.Conventions;

namespace InstallChecker.Identity.Signaux;

/// <summary>Un type de signal σ = (D_σ, P_σ, f_σ, K_σ) — 002 Déf. 1. Identité : son nom + la convention qui le fonde.</summary>
public sealed record TypeDeSignal(string Identifiant, ConventionRef Convention);
