namespace InstallChecker.Identity.Observations;

/// <summary>Une valeur observée : un texte, un entier, ou ⊥ (absence) — 014 § 6, 001 Déf. 2.</summary>
public abstract record ValeurObservee
{
    private ValeurObservee() { }

    public sealed record Texte(string Valeur) : ValeurObservee;

    public sealed record Entier(long Valeur) : ValeurObservee;

    public sealed record Absente : ValeurObservee
    {
        public static readonly Absente Instance = new();
    }
}
