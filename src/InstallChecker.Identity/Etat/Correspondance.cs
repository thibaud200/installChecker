namespace InstallChecker.Identity.Etat;

/// <summary>
/// Le classement exhaustif du contenu de W′ par rapport à W (006 § 7 ; 014 § 7.5) : conservé,
/// abandonné, nouveau — chaque référence identifiée par (strate, plus petit identifiant du domaine) —
/// et les continuités déclarées (006 § 5).
/// </summary>
// ponytail: Continuites est toujours vide ici — aucune élection de strate identité n'existe encore
// dans ce moteur (seule la strate contenu est décidée) ; la continuité ne concerne que ce niveau
// (006 § 5). À peupler quand C4/C5 éliront une première identité.
public sealed record Correspondance(
    IReadOnlyList<ReferenceActe> Conserves,
    IReadOnlyList<ReferenceActe> Abandonnes,
    IReadOnlyList<ReferenceActe> Nouveaux,
    IReadOnlyList<(ReferenceActe Avant, ReferenceActe Apres)> Continuites);
