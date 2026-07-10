namespace InstallChecker.Identity.Etat;

/// <summary>
/// Le classement exhaustif du contenu de W′ par rapport à W (006 § 7 ; 014 § 7.5) : conservé,
/// abandonné, nouveau — chaque référence étant l'identité de l'acte (024 § 3) — et les continuités
/// déclarées, dérivées par C6 selon le critère du 006 § 5 (026 § 4) : entre élections, même strate,
/// même contenu propositionnel, domaines se recouvrant — les triviales des conservés comprises (006 E5).
/// </summary>
public sealed record Correspondance(
    IReadOnlyList<ReferenceActe> Conserves,
    IReadOnlyList<ReferenceActe> Abandonnes,
    IReadOnlyList<ReferenceActe> Nouveaux,
    IReadOnlyList<(ReferenceActe Avant, ReferenceActe Apres)> Continuites);
