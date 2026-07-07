namespace InstallChecker.Identity.Etat;

/// <summary>La cause exacte d'une transition (014 § 7.5) : son type, et le détail (actes ajoutés, ou transition de convention) — fourni par l'appelant, jamais déduit par C6 (006 § 6).</summary>
public sealed record Cause(TypeCause Type, string Detail);
