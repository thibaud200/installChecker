namespace InstallChecker.DuplicateFiles;

public enum CategorieRedondanceVersionnee
{
    VersionRedundancyCandidate,
}

public enum RoleComparaisonVersionnee
{
    ReferenceRecente,
    VersionAnterieure,
    MemeVersion,
}

public enum ActionVersionnee
{
    Examiner,
    Ignorer,
}

public enum RaisonBlocageVersionnee
{
    RevueHumaineObligatoire,
    SuppressionAutomatiqueInterdite,
    ConfianceFaible,
    VarianteNonObservee,
    MetadonneesContradictoires,
}

public enum MotifExclusionVersionnee
{
    FamilleInsuffisante,
    AucuneVersion,
    VersionNonComparable,
    ConflitDeVersion,
    ConflitDeFamille,
    VarianteIncompatible,
    MemeVersionSeulement,
}

public sealed record SourceRapportVersionnee(int NombreFichiersObserves);

public sealed record SyntheseRedondanceVersionnee(
    int NombreGroupes,
    int NombreContenus,
    int NombreReferencesRecentes,
    int NombreVersionsAnterieures,
    int NombreConflits);

public sealed record FichierVersionneRapporte(string FichierId, string Chemin, long Taille);

public sealed record ArtefactVersionneRapporte(
    string ContenuSha256,
    IReadOnlyList<FichierVersionneRapporte> Fichiers,
    string Version,
    RoleComparaisonVersionnee? Role,
    IReadOnlyList<PreuveVersionnee> Preuves,
    IReadOnlyList<DiagnosticVersionne> Diagnostics,
    IReadOnlyList<ActionVersionnee> Actions,
    IReadOnlyList<RaisonBlocageVersionnee> Blocages);

public sealed record GroupeRedondanceVersionnee(
    string GroupeId,
    CategorieRedondanceVersionnee Categorie,
    string Famille,
    VarianteVersionnee Variante,
    NiveauConfianceVersionnee Confiance,
    string VersionReference,
    IReadOnlyList<ArtefactVersionneRapporte> Artefacts,
    IReadOnlyList<RaisonBlocageVersionnee> Blocages);

public sealed record RapportRedondanceVersionnee(
    string VersionContrat,
    SourceRapportVersionnee Source,
    SyntheseRedondanceVersionnee Synthese,
    IReadOnlyList<GroupeRedondanceVersionnee> Groupes,
    IReadOnlyDictionary<MotifExclusionVersionnee, int> ExclusionsParMotif);
