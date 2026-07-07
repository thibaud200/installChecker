namespace InstallChecker.Identity.Hypotheses;

/// <summary>Les cinq niveaux de la chaîne de stratification (005 § 3). Seul <see cref="Contenu"/> est produit à ce jour.</summary>
public enum Strate
{
    Contenu,
    Variante,
    Version,
    Identite,
    Famille,
}
