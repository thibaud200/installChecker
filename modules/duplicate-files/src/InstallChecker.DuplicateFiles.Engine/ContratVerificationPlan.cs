namespace InstallChecker.DuplicateFiles;

public enum ModeVerificationPlan
{
    Simulation,
}

public enum EtatLectureFichier
{
    Disponible,
    Absent,
    Illisible,
    TypeNonPrisEnCharge,
}

public enum EtatVerificationFichier
{
    Valide,
    Absent,
    Illisible,
    HashDifferent,
    CheminProtege,
    TypeNonPrisEnCharge,
}

public enum RoleFichierPlan
{
    TemoinConservation,
    Candidat,
}

public enum EtapeJournalVerificationPlan
{
    VerifierTemoin,
    VerifierCandidat,
}

public sealed record ObservationFichierCourant(
    EtatLectureFichier Etat,
    string? HashObserve,
    string? Detail);

public interface IObservateurDeFichier
{
    ObservationFichierCourant Observer(string chemin);
}

public sealed record VerificationFichierPlan(
    string GroupeId,
    string FichierId,
    string Chemin,
    string HashAttendu,
    string? HashObserve,
    RoleFichierPlan Role,
    EtatVerificationFichier Etat,
    string? Detail);

public sealed record VerificationGroupePlan(
    string GroupeId,
    bool Executable,
    IReadOnlyList<EtatVerificationFichier> Blocages,
    IReadOnlyList<VerificationFichierPlan> Fichiers);

public sealed record EntreeJournalVerificationPlan(
    int Sequence,
    string GroupeId,
    string FichierId,
    EtapeJournalVerificationPlan Etape,
    EtatVerificationFichier Etat);

public sealed record RapportDeVerificationPlan(
    string VersionContrat,
    ModeVerificationPlan Mode,
    bool Executable,
    IReadOnlyList<VerificationGroupePlan> Groupes,
    IReadOnlyList<EntreeJournalVerificationPlan> Journal);
