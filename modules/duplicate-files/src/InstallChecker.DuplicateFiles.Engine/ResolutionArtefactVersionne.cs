namespace InstallChecker.DuplicateFiles;

public enum NiveauConfianceVersionnee
{
    Faible,
    Moyenne,
    Forte,
}

public enum EtatResolutionVersionnee
{
    Comparable,
    VersionNonComparable,
    ConflitDeVersion,
    ConflitDeFamille,
}

public sealed record VarianteVersionnee(
    string Format,
    string? Architecture,
    string? Langue,
    string? Edition,
    string? Distribution,
    bool Partielle);

public sealed record ArtefactVersionne(
    string ContenuSha256,
    IReadOnlyList<FichierObserve> Fichiers,
    string? CleFamille,
    string? LibelleFamille,
    SourcePreuveVersionnee? SourceFamille,
    VersionComparable? Version,
    VarianteVersionnee Variante,
    NiveauConfianceVersionnee Confiance,
    EtatResolutionVersionnee Etat,
    IReadOnlyList<PreuveVersionnee> Preuves,
    IReadOnlyList<DiagnosticVersionne> Diagnostics);
