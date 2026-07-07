namespace InstallChecker.Identity.Etat;

/// <summary>
/// L'état du monde W (006 Déf. 2 ; 012 § 5 ; 014 § 7.1) : l'index, puis la totalité des actes, en
/// ordre canonique — rien d'autre (pas de chaînes, pas de contexte, pas de métadonnées de calcul).
/// </summary>
public sealed record W(IndexEtat Index, IReadOnlyList<ActeW> Actes);
