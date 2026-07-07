namespace InstallChecker.Identity.Conventions;

/// <summary>Le couple (identifiant, version) qui identifie une convention (004 Déf. 1).</summary>
public sealed record ConventionRef(string Identifiant, int Version);
