namespace InstallChecker.Identity.Conventions;

/// <summary>
/// Le référentiel de conventions produit par C2 (014 § 1) : les conventions en vigueur,
/// et l'identité de l'état de ℛ qui s'en déduit (014 § 7.2 — la liste triée des couples en vigueur).
/// </summary>
// ponytail: pas de champ « incompatibilités déclarées » — la grammaire définitive d'etat.md (015 § 7.2)
// n'en prévoit aucune section ; à ajouter quand une convention future en déclarera une (007 § 6).
public sealed record Referentiel(IReadOnlyList<Convention> ConventionsEnVigueur)
{
    public IReadOnlyList<ConventionRef> Index =>
        ConventionsEnVigueur
            .Select(c => c.Ref)
            .OrderBy(r => r.Identifiant, StringComparer.Ordinal)
            .ToList();
}
