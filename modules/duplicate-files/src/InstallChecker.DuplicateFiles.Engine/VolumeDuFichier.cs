namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Le volume porteur d'un exemplaire, tel qu'observé au scan (spec multi-disque D5) : avec des
/// lettres de lecteur changeantes, c'est lui — pas le chemin — qui dit sur quel disque physique
/// se trouve chaque copie. DTO du module — aucun type moteur.
/// </summary>
public sealed record VolumeDuFichier(string VolumeId, string? VolumeLabel);
