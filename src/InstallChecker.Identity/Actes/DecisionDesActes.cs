using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.Identity.Actes;

/// <summary>
/// C5 — décision des actes (012 § 1.2, 014 C5). Fonction pure : (hypothèses de C4, référentiel de
/// C2) → ensemble des actes. Ne relit jamais les signaux ni les observations (012 § 3 : C5 décide
/// sur les objets d'hypothèses, qui portent déjà régimes et provenances) et ne calcule aucune
/// hypothèse — l'espace lui est donné par C4.
///
/// Seule convention d'élection connue à ce jour : CE-01 (007 § 4, adoptée au 009 § 4). Sa portée
/// est exactement la strate contenu (005 § 3) : sous ℛ₀, chaque hypothèse produite par C4 est de
/// cette strate, soutenue par un signal en régime exact (EQ-01 n'en produit pas d'autre), et sa
/// maximale est structurellement unique (003 § 9, consensus dégénéré) — la configuration que CE-01
/// licencie est donc toujours réalisée, sans qu'aucune comparaison entre hypothèses ne soit jamais
/// nécessaire ici (I25 : C5 ne fabrique aucun candidat, il sélectionne).
///
/// En l'absence de CE-01 en vigueur, aucune élection n'est de plein droit (I27, 007 § 3) : chaque
/// hypothèse formulable retombe en refus normatif — « configuration licenciable, aucune convention
/// adoptée » (motif normalisé `licenciable-non-licencié`, 014 § 7.4).
/// </summary>
public static class DecisionDesActes
{
    private const string IdentifiantCE01 = "CE-01";
    private const string MotifUniqueMaximale = "unique-maximale";
    private const string MotifLicenciableNonLicencie = "licenciable-non-licencié";

    public static EnsembleDesActes Decider(IReadOnlyList<Hypothese> hypotheses, Referentiel referentiel)
    {
        var ce01 = referentiel.ConventionsEnVigueur.SingleOrDefault(
            c => c.Identifiant == IdentifiantCE01 && c.Famille == Famille.Election);

        var hypothesesTriees = hypotheses.OrderBy(h => h.Domaine[0]).ToList();

        if (ce01 is null)
        {
            var refus = hypothesesTriees
                .Select(h => new Refus(h.Strate, h.Domaine, Espece.Normatif, MotifLicenciableNonLicencie))
                .ToList();
            return new EnsembleDesActes([], refus);
        }

        var elections = hypothesesTriees.Select(h => Elire(h, ce01)).ToList();
        return new EnsembleDesActes(elections, []);
    }

    private static ActeElection Elire(Hypothese hypothese, Convention ce01)
    {
        var dependances = hypothese.ConventionsMobilisees
            .Append(ce01.Ref)
            .Distinct()
            .OrderBy(c => c.Identifiant, StringComparer.Ordinal)
            .ThenBy(c => c.Version)
            .ToList();

        return new ActeElection(
            hypothese.Strate,
            hypothese.Domaine,
            hypothese.ContenuPropositionnel,
            Niveau.Certaine,
            MotifUniqueMaximale,
            Licences: [ce01.Ref],
            Dependances: dependances,
            Dette: []);
    }
}
