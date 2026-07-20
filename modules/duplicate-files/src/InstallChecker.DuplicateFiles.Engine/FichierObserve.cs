using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles;

public sealed record FichierObserve(
    long ActeId,
    string FichierId,
    string Chemin,
    long Taille,
    string ContenuSha256,
    IReadOnlyDictionary<Attribut, ValeurObservee> Attributs);
