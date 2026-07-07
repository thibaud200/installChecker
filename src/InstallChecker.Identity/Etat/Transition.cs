namespace InstallChecker.Identity.Etat;

/// <summary>τ = (W → W′, cause, correspondance) — 006 Déf. 7 ; 014 § 7.5.</summary>
public sealed record Transition(IndexEtat IndexAvant, IndexEtat IndexApres, Cause Cause, Correspondance Correspondance);
