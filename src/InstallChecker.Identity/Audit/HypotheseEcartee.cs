namespace InstallChecker.Identity.Audit;

/// <summary>Une hypothèse concurrente écartée (011 § 7, 003 § 1.3) : son contenu propositionnel et le motif de son écartement.</summary>
public sealed record HypotheseEcartee(string ContenuPropositionnel, string MotifEcartement);
