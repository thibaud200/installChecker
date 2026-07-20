namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Un exemplaire tel que restitué dans le rapport (plan rev3 § 2.3) : le fichier, son rang, et
/// l'étiquette lisible « à conserver » (rang 1) ou « candidat à la suppression » (rang ≥ 2) —
/// reformulation du rang brut, le classement lui-même restant celui de <see cref="PolitiqueRetentionV1"/>.
/// Le motif historique, les critères structurés, le rôle et l'état des actions sont conservés pour
/// l'explicabilité et une future interface. DTO du module — aucun type moteur.
/// </summary>
public sealed record ExemplaireRapporte(
    FichierEnrichi Fichier,
    int Rang,
    string Etiquette,
    string Motif,
    string FichierId,
    RoleExemplaire Role,
    IReadOnlyList<CritereClassement> CriteresClassement,
    IReadOnlyList<EtatActionFichier> Actions);
