namespace InstallChecker.Identity.Observations;

/// <summary>
/// Le contexte d'un acte (localisation, datation — A1) : jamais consommé par la dérivation,
/// restitué exclusivement par C7 (014 C1 : « produit... sur un canal séparé destiné exclusivement à C7 »).
/// </summary>
public sealed record ContexteObservation(long Identifiant, string Chemin, string DateDeScan);
