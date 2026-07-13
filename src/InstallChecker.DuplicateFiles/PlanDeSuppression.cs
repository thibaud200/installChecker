namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Une proposition de suppression (spec A3) : le chemin d'une copie redondante dont la suppression
/// est proposée, étiqueté de l'empreinte du contenu dont il est une copie. Le contenu rattache la
/// proposition sans décrire le groupe : ni les autres chemins, ni ceux qui subsistent n'y figurent.
/// </summary>
public sealed record PropositionDeSuppression(string Contenu, string Chemin);

/// <summary>
/// Le plan de suppression (spec A3) : la liste plate des propositions. Le plan ne décrit que des
/// propositions — jamais les groupes, jamais les chemins conservés. Rien n'est exécuté.
/// </summary>
public sealed record PlanDeSuppression(IReadOnlyList<PropositionDeSuppression> Propositions);
