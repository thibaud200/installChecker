namespace InstallChecker.Identity.Audit;

/// <summary>
/// Une chaîne d'audit (008 § 5, 012 § 6) : la suite des maillons effectivement franchis. Une chaîne
/// aboutie porte <see cref="ManqueNomme"/> nul ; une chaîne interrompue le porte — le nom exact de ce
/// qui manque au maillon suivant, jamais franchi. Les deux formes sont des objets complets, de même
/// dignité (012 § 6 : « une chaîne interrompue est toujours valide »).
/// </summary>
public sealed record Chaine(IReadOnlyList<Maillon> Maillons, string? ManqueNomme);
