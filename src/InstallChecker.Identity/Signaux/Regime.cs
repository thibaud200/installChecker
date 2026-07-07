namespace InstallChecker.Identity.Signaux;

/// <summary>Le régime d'une instance de signal (002 § 5) — catégoriel, jamais un ordre numérique (I9).</summary>
public enum Regime
{
    Exact,
    Incomplet,
    Ambigu,
    Contradictoire,
    Artefactuel,
}
