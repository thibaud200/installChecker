namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Un exemplaire tel que restitué dans le rapport (plan rev3 § 2.3) : le fichier, son rang, et
/// l'étiquette lisible « à conserver » (rang 1) ou « candidat à la suppression » (rang ≥ 2) —
/// reformulation du rang brut, le classement lui-même restant celui de <see cref="PolitiqueRetentionV1"/>.
/// Le motif du classement est conservé pour l'explicabilité. DTO du module — aucun type moteur.
/// </summary>
public sealed record ExemplaireRapporte(FichierEnrichi Fichier, int Rang, string Etiquette, string Motif);
