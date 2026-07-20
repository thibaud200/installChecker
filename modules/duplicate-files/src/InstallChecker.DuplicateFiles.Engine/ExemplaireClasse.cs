namespace InstallChecker.DuplicateFiles;

/// <summary>Un fichier classé au sein d'un groupe (conception D3) : Rang 1 est l'exemplaire suggéré à conserver — jamais une décision automatique (D3 : « l'utilisateur choisit »). L'étiquette lisible « à conserver / candidat à la suppression » est une reformulation du rang produite à l'assemblage du rapport (plan rev3 § 2.3), pas ici.</summary>
public sealed record ExemplaireClasse(FichierEnrichi Fichier, int Rang, string Motif);
