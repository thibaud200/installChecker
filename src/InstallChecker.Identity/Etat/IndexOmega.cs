namespace InstallChecker.Identity.Etat;

/// <summary>
/// L'identité de l'état d'Ω dans l'index (014 § 7.2) : la version de contrat supportée, le nombre
/// d'actes, et l'empreinte d'état — la fonction d'empreinte du support appliquée à la concaténation,
/// en ordre canonique d'identifiants, des empreintes de contenu déjà présentes dans Ω (aucune
/// fonction nouvelle introduite).
/// </summary>
public sealed record IndexOmega(long Version, int NombreActes, string EmpreinteEtat);
