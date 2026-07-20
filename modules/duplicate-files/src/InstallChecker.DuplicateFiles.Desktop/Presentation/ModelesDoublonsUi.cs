namespace InstallChecker.DuplicateFiles.Desktop.Presentation;

public sealed record FichierDoublonUi(
    string FichierId,
    long ActeId,
    string Chemin,
    long Taille,
    int Rang,
    string Role,
    string? Volume,
    IReadOnlyList<string> Blocages);

public sealed record GroupeDoublonUi(
    string GroupeId,
    long TailleUnitaire,
    long EspaceRecuperable,
    string Confiance,
    string? Sha256,
    IReadOnlyList<FichierDoublonUi> Fichiers);

public sealed record RapportDoublonsUi(
    bool EstHistorique,
    int NombreGroupes,
    int NombreCandidats,
    long EspaceRecuperable,
    IReadOnlyList<GroupeDoublonUi> Groupes);
