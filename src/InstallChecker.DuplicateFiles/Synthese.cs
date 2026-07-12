namespace InstallChecker.DuplicateFiles;

/// <summary>
/// La synthèse de bibliothèque (plan rev3 § 2.1) : les cinq agrégats métier mis en avant en tête de
/// rapport, avant le détail des groupes. DTO du module — aucun type moteur.
/// </summary>
public sealed record Synthese(
    int NombreDeGroupes,
    int NombreDeFichiersRedondants,
    long EspaceRecuperableOctets,
    int NombreDeFichiersAConserver,
    int NombreDeCandidatsASuppression);
