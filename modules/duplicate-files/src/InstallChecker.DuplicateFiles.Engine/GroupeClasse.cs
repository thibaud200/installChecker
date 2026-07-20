namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Un groupe de doublons prêt à être restitué (plan rev3 § 2.3, conception D5) : le domaine des
/// actes, le motif court d'identité, les métriques par groupe (taille unitaire et espace récupérable
/// = taille × (n − 1), calculées par <see cref="GenerateurDeRapport"/> — correction B), et les
/// exemplaires classés et étiquetés, les identifiants stables et la preuve structurée du contenu
/// identique. La chaîne d'audit complète du moteur reste accessible à la
/// demande via la commande <c>identity audit</c>, jamais dupliquée ici. DTO du module — aucun type moteur.
/// </summary>
public sealed record GroupeClasse(
    IReadOnlyList<long> Domaine,
    string MotifCourt,
    long TailleUnitaire,
    long EspaceRecuperableOctets,
    IReadOnlyList<ExemplaireRapporte> Exemplaires,
    string GroupeId,
    CategorieDoublon Categorie,
    NiveauConfiance Confiance,
    string ContenuSha256,
    IReadOnlyList<PreuveDoublon> Preuves,
    string FichierRecommandeId);
