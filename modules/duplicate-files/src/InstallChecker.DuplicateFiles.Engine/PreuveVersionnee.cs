namespace InstallChecker.DuplicateFiles;

public enum DimensionPreuveVersionnee
{
    CleFamille,
    LibelleFamille,
    IdentifiantLivraison,
    Version,
    Editeur,
    Format,
    Architecture,
    Langue,
    Edition,
    Distribution,
}

public enum SourcePreuveVersionnee
{
    NomFichier,
    VersionInfo,
    Msi,
    Appx,
    Pe,
    Authenticode,
    Arbitre,
}

public enum ForcePreuveVersionnee
{
    Faible,
    Moyenne,
    Forte,
}

public enum CodeDiagnosticVersionne
{
    AttributInvalide,
    VersionNonComparable,
    ConflitDeVersion,
    ConflitDeFamille,
    VarianteNonObservee,
}

public sealed record PreuveVersionnee(
    string FichierId,
    DimensionPreuveVersionnee Dimension,
    string ValeurBrute,
    string ValeurNormalisee,
    SourcePreuveVersionnee Source,
    ForcePreuveVersionnee Force,
    string Regle,
    string VersionFournisseur);

public sealed record DiagnosticVersionne(
    string FichierId,
    CodeDiagnosticVersionne Code,
    SourcePreuveVersionnee Source,
    string DetailNormalise);

public sealed record ResultatFournisseur(
    IReadOnlyList<PreuveVersionnee> Preuves,
    IReadOnlyList<DiagnosticVersionne> Diagnostics)
{
    public static readonly ResultatFournisseur Vide = new([], []);
}
