using InstallChecker.Identity.Conventions;

namespace InstallChecker.Identity.Etat;

/// <summary>
/// L'index d'un état du monde (014 § 7.2) : l'identité de l'état d'Ω et la liste explicite, triée
/// par identifiant, des couples (identifiant, version) en vigueur du registre — <see cref="Conventions.Referentiel.Index"/>
/// produit déjà exactement cette forme.
/// </summary>
public sealed record IndexEtat(IndexOmega Omega, IReadOnlyList<ConventionRef> Registre);
