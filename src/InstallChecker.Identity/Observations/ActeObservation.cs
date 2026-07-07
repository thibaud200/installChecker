namespace InstallChecker.Identity.Observations;

/// <summary>
/// Un acte d'observation complet (014 § 2) : son identifiant, ses attributs de contenu
/// (taille, empreinte — 014 § 6), et la famille complète de ses observations (invariant 1:1).
/// Ne porte aucun attribut de contexte (A1) — voir <see cref="ContexteObservation"/>.
/// </summary>
public sealed record ActeObservation(
    long Identifiant,
    long Taille,
    string Empreinte,
    IReadOnlyDictionary<Attribut, ValeurObservee> Attributs);
