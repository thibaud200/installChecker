namespace InstallChecker.DuplicateFiles.Desktop.Presentation;

public sealed record ArtefactVersionUi(
    string ContenuSha256,
    string Version,
    string? Role,
    IReadOnlyList<string> Chemins,
    IReadOnlyList<string> Blocages);

public sealed record GroupeVersionUi(
    string GroupeId,
    string Famille,
    string VersionReference,
    string Confiance,
    string Format,
    string? Architecture,
    string? Langue,
    bool VariantePartielle,
    IReadOnlyList<string> Blocages,
    IReadOnlyList<ArtefactVersionUi> Artefacts);

public sealed record RapportVersionsUi(
    int NombreGroupes,
    int NombreVersionsAnterieures,
    IReadOnlyList<GroupeVersionUi> Groupes);
