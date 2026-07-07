using InstallChecker.Identity.Conventions;

namespace InstallChecker.Identity.Audit;

/// <summary>
/// Réponse à « que faudrait-il renier pour que ceci tombe ? » (011 § 7, 008 § 8) : les ensembles
/// minimaux de conventions de l'acte, comparables par inclusion (008 § 8), et sa dette (006 § 9).
/// </summary>
public sealed record EnsemblesMinimauxReponse(IReadOnlyList<IReadOnlyList<ConventionRef>> EnsemblesMinimaux, IReadOnlyList<ConventionRef> Dette);
