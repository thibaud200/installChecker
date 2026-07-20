namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Une proposition de suppression (spec A3) : le chemin d'une copie redondante dont la suppression
/// est proposée, étiqueté de l'empreinte du contenu dont il est une copie. Le contenu rattache la
/// proposition sans décrire le groupe : ni les autres chemins, ni ceux qui subsistent n'y figurent.
/// Les identifiants stables permettent de retrouver le même groupe et le même fichier dans le rapport.
/// </summary>
public sealed record PropositionDeSuppression(
    string Contenu,
    string Chemin,
    string GroupeId,
    string FichierId);

public sealed record TemoinDeConservation(string FichierId, string Chemin);

public sealed record GarantieDeGroupe(
    string GroupeId,
    string ContenuSha256,
    TemoinDeConservation TemoinConservation);

/// <summary>
/// Le plan de suppression (spec A3) : la liste plate des propositions. Le plan ne décrit que des
/// propositions — jamais les groupes, jamais les chemins conservés. Rien n'est exécuté.
/// </summary>
public sealed record PlanDeSuppression(
    IReadOnlyList<PropositionDeSuppression> Propositions,
    string VersionContrat,
    IReadOnlyList<GarantieDeGroupe> GarantiesParGroupe);
