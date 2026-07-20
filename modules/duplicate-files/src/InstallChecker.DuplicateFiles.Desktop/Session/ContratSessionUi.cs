using System.Text.Json;

namespace InstallChecker.DuplicateFiles.Desktop.Session;

public static class VersionsContratUi
{
    public const string SessionV1 = "duplicate-files/desktop-session/v1";
}

public enum EtatRevueUi
{
    AExaminer,
    Conserver,
    Prevoir,
    Ignorer
}

public sealed record BibliothequeUi(
    string Nom,
    string CheminBase,
    string CheminRegistre,
    IReadOnlyList<string> Racines,
    string CheminSession);

public sealed record DecisionRevueUi(
    string GroupeId,
    string? FichierId,
    EtatRevueUi Etat,
    DateTimeOffset ModifieeLe);

public sealed record DiagnosticUi(string Code, string Message, string? Chemin = null);

public sealed record EtatFiltresUi(string Recherche, EtatRevueUi? Etat, string? Confiance);

public sealed record SessionDuplicateFilesUi(
    string VersionContrat,
    BibliothequeUi Bibliotheque,
    DateTimeOffset? DernierScan,
    JsonElement? RapportDoublons,
    JsonElement? RapportVersions,
    IReadOnlyDictionary<string, DecisionRevueUi> Decisions,
    EtatFiltresUi FiltresDoublons,
    EtatFiltresUi FiltresVersions,
    IReadOnlyList<DiagnosticUi> Diagnostics);
