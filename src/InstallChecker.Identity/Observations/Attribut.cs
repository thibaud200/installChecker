namespace InstallChecker.Identity.Observations;

/// <summary>L'identité d'un attribut : le nom de sa capacité et son propre nom (001 Déf. 4, § Provenance).</summary>
public sealed record Attribut(string Capacite, string Nom);
