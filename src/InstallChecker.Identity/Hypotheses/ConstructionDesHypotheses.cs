using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Hypotheses;

/// <summary>
/// C4 — construction des hypothèses (012 § 1.2, 014 C4). Fonction pure : instances de signaux →
/// hypothèses. Ne lit jamais le modèle d'observations (012 § 3 : « C4 ne lit jamais Ω directement ») —
/// tout ce qu'une hypothèse affirme se reconstruit depuis les signaux qui la fondent.
///
/// Strate produite à ce jour : contenu, exclusivement. C'est la seule strate dont la granularité
/// n'exige aucune convention de stratification (005 § 3 : « la seule strate observée » — l'équivalence
/// ≡ₘ elle-même) ; le référentiel ℛ₀ n'en déclare d'ailleurs aucune. Une classe de contenu (un
/// ensemble d'actes de même empreinte) est un consensus dégénéré (003 § 9) : un seul type de signal,
/// aucun résidu — donc une hypothèse unique par classe, jamais comparée, jamais élue.
///
/// C4 ne choisit rien, ne compare rien, ne calcule aucun niveau de certitude et ne produit aucun
/// refus : ce sont les responsabilités de C5/C6 (006, 012 § 1.2).
/// </summary>
internal static class ConstructionDesHypotheses
{
    private const string TypeContenuIdentique = "contenu-identique";

    public static IReadOnlyList<Hypothese> Construire(IReadOnlyList<InstanceDeSignal> signaux) =>
        signaux
            .Where(s => s.Type.Identifiant == TypeContenuIdentique)
            .GroupBy(s => s.Sortie, StringComparer.Ordinal)
            .Select(CreerHypothese)
            .OrderBy(h => h.Domaine[0])
            .ToList();

    private static Hypothese CreerHypothese(IGrouping<string, InstanceDeSignal> classe)
    {
        var sig = classe
            .OrderBy(s => s.Provenance[0].ActeId)
            .ThenBy(s => s.Provenance[1].ActeId)
            .ToList();

        var domaine = sig
            .SelectMany(s => s.Provenance.Select(o => o.ActeId))
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var obs = sig
            .SelectMany(s => s.Provenance)
            .Distinct()
            .OrderBy(o => o.ActeId)
            .ThenBy(o => o.Attribut, StringComparer.Ordinal)
            .ToList();

        var justification =
            $"consensus dégénéré (003 § 9) : {sig.Count} signal(aux) « {TypeContenuIdentique} », classe d'empreinte {classe.Key}";

        return new Hypothese(Strate.Contenu, domaine, classe.Key, obs, sig, justification);
    }
}
