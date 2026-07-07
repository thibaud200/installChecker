namespace InstallChecker.Identity.Conventions;

/// <summary>
/// Le contenu logique complet d'une version d'une convention (014 § 5.1).
/// Projection pure du fichier de version — aucun champ n'est interprété au-delà de son type déclaré.
/// </summary>
public sealed record Convention(
    string Identifiant,
    int Version,
    Famille Famille,
    string DomaineApplication,
    string Transformation,
    IReadOnlyList<ConventionRef> Dependances,
    string RegimesAdmis,
    string Portee,
    string Justification,
    string JustificationEmpirique,
    string Limites,
    string ConditionsDeRevision,
    DateOnly Date,
    string Autorite)
{
    public ConventionRef Ref => new(Identifiant, Version);
}
