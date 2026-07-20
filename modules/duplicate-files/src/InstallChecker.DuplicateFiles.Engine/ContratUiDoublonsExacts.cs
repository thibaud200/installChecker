namespace InstallChecker.DuplicateFiles;

public static class VersionsContratDuplicateFiles
{
    public const string DoublonsExactsV1 = "duplicate-files/exact-duplicates/v1";
    public const string PlanSecuriseV1 = "duplicate-files/safe-plan/v1";
    public const string VerificationPlanV1 = "duplicate-files/safe-plan-verification/v1";
    public const string RedondanceVersionneeV1 = "duplicate-files/version-redundancy/v1";
}

public enum CategorieDoublon
{
    ExactDuplicate,
}

public enum NiveauConfiance
{
    Certaine,
}

public enum TypePreuveDoublon
{
    Sha256Identique,
}

public enum RoleExemplaire
{
    RecommandeAConserver,
    Candidat,
}

public enum CritereRetention
{
    RichesseObservations,
    NomDeCopie,
    DateObservation,
    Chemin,
    ActeIdDepartage,
}

public enum ActionFichier
{
    Conserver,
    AjouterAuPlanDeSuppression,
}

public enum RaisonBlocageAction
{
    FichierRecommandeAConserver,
    CheminProtege,
}

public sealed record PreuveDoublon(TypePreuveDoublon Type, string Valeur);

public sealed record CritereClassement(CritereRetention Critere, int Priorite, string Valeur);

public sealed record EtatActionFichier(
    ActionFichier Action,
    bool Autorisee,
    IReadOnlyList<RaisonBlocageAction> Blocages);
