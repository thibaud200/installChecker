using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.Identity.Etat;

/// <summary>Une référence d'acte dans une correspondance (014 § 7.5) : le couple (strate, plus petit identifiant du domaine).</summary>
public readonly record struct ReferenceActe(Strate Strate, long PlusPetitIdentifiantDuDomaine);
