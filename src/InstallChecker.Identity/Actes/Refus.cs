using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.Identity.Actes;

/// <summary>
/// Un refus de conclure r = (D, 𝒮, (Ω, K), motif structurel) — 006, Déf. 4. Résultat positif de
/// plein droit (pas une exception, pas un manque) : aucune hypothèse n'est retenue sur ce domaine,
/// à cette strate, sous cet index. Un refus ne doit rien (006 § 9) — il ne porte ni dette ni niveau.
/// </summary>
public sealed record Refus(
    Strate Strate,
    IReadOnlyList<long> Domaine,
    Espece Espece,
    string Motif);
