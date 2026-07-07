namespace InstallChecker.Identity.Audit;

/// <summary>Le statut d'un maillon (012 § 6) : nominal, ou porteur d'un régime, d'une contradiction ou d'une dette — jamais un jugement, un fait déjà porté par l'objet produit à cette couche.</summary>
public enum StatutMaillon
{
    Nominal,
    PorteRegime,
    PorteContradiction,
    PorteDette,
}
