using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Hypotheses;

/// <summary>
/// Une hypothèse d'identité h = (Dom, Obs, Sig, prov, just) — 003, Déf. 1. Elle ne porte aucune
/// certitude intrinsèque (003 § 1.2) : ni niveau, ni poids, ni comparaison avec une autre hypothèse.
/// La provenance jusqu'aux actes est portée par <see cref="Sig"/> (chaque instance cite ses
/// observations consommées, I7) — aucun champ séparé n'est nécessaire (I10 : rien de reconstructible
/// n'a besoin d'être stocké deux fois).
/// </summary>
public sealed record Hypothese(
    Strate Strate,
    IReadOnlyList<long> Domaine,
    string ContenuPropositionnel,
    IReadOnlyList<ObservationConsommee> Obs,
    IReadOnlyList<InstanceDeSignal> Sig,
    string Justification)
{
    /// <summary>Les conventions mobilisées (003 § 1.1) : reconstruites depuis les types de signaux de <see cref="Sig"/>, jamais stockées à part (I10).</summary>
    public IReadOnlyList<ConventionRef> ConventionsMobilisees =>
        Sig
            .Select(s => s.Type.Convention)
            .Distinct()
            .OrderBy(c => c.Identifiant, StringComparer.Ordinal)
            .ThenBy(c => c.Version)
            .ToList();
}
