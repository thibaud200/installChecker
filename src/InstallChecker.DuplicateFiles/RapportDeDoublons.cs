namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Le rapport complet du module (plan rev3 § 2) : la <see cref="Synthese"/> de bibliothèque en tête,
/// puis la <see cref="NoteDeCapacite"/> (absente lorsque aucune strate supérieure ne refuse), puis
/// le détail des <see cref="GroupeClasse"/>. Plus aucun champ <c>NonTranches</c> typé moteur
/// (corrections P2/P4) : la sortie n'expose que des DTO du module.
/// </summary>
public sealed record RapportDeDoublons(
    Synthese Synthese,
    NoteDeCapacite? Note,
    IReadOnlyList<GroupeClasse> Groupes);
