using InstallChecker.Identity.Erreurs;

namespace InstallChecker.Identity.Conventions;

/// <summary>
/// La quatrième vérification de la frontière registre (017 § 4) : toute convention en vigueur
/// de ℛ appartient à une famille de la couverture déclarée du moteur invoqué. Elle appartient
/// à C2 (017 § 5) et se vérifie en dernier — après l'absence, la forme et la cohérence
/// (017 § 8) : en aval, tout référentiel est couvert par construction (I51 étendu).
/// Son échec est la septième erreur du contrat (017 § 6) — jamais un refus, jamais un W partiel (I61).
/// </summary>
internal static class VerificationDeCouverture
{
    public static void Verifier(IReadOnlyList<Convention> conventionsEnVigueur)
    {
        var horsCouverture = conventionsEnVigueur
            .Where(c => !DeclarationDeCouverture.Couvre(c.Famille))
            .OrderBy(c => c.Identifiant, StringComparer.Ordinal)
            .ThenBy(c => c.Version)
            .ToList();

        if (horsCouverture.Count == 0)
        {
            return;
        }

        // Diagnostic contractuel (017 § 6) : l'entrée fautive (ℛ — et, en son sein, la ou les
        // conventions concernées avec leur famille) et la clause violée (la quatrième
        // précondition, 017 § 4). Tri déterministe : même registre, même message (I64).
        var fautives = string.Join(
            " ; ",
            horsCouverture.Select(c => $"{c.Identifiant} v{c.Version} (famille {c.Famille})"));
        var couvertes = string.Join(", ", DeclarationDeCouverture.FamillesCouvertes.Order());

        throw new RegistreNonCouvertException(
            $"registre non couvert : {fautives} — hors de la couverture déclarée du moteur " +
            $"({couvertes}) ; quatrième précondition d'invocation violée (017 § 4)");
    }
}
