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
///
/// Sous ℛ₀, aucune convention d'interprétation ou de stratification n'existe au-delà de la strate
/// contenu (009 § 6) : les strates variante, version et identité n'ont donc jamais de signal ni de
/// licence qui les fonde — refus normatif `aucune-convention-strate` (014 § 7.4, § 8). La strate
/// famille exige des identités déjà retenues (aucune ne l'est jamais ici) — refus normatif
/// `préalable-absent`. Ces quatre refus sont un fait structurel de ℛ₀ (aucune autre famille de
/// convention n'existe dans ce registre) : ils ne dépendent d'aucune hypothèse ni d'aucune
/// comparaison, et couvrent toujours le domaine maximal (la totalité des actes d'Ω, seule
/// information nécessaire ici — Obs(h) et le contenu des actes restent hors de portée de C5, I51).
/// </summary>
public static class DecisionDesActes
{
    private const string IdentifiantCE01 = "CE-01";
    private const string MotifUniqueMaximale = "unique-maximale";
    private const string MotifLicenciableNonLicencie = "licenciable-non-licencié";
    private const string MotifAucuneConventionStrate = "aucune-convention-strate";
    private const string MotifPrealableAbsent = "préalable-absent";

    public static EnsembleDesActes Decider(
        IReadOnlyList<Hypothese> hypotheses, Referentiel referentiel, IReadOnlyList<long> identifiantsDesActes)
    {
        var ce01 = referentiel.ConventionsEnVigueur.SingleOrDefault(
            c => c.Identifiant == IdentifiantCE01 && c.Famille == Famille.Election);

        var hypothesesTriees = hypotheses.OrderBy(h => h.Domaine[0]).ToList();
        var refusDesStratesSuperieures = RefusDesStratesSuperieures(identifiantsDesActes);

        if (ce01 is null)
        {
            var refusContenu = hypothesesTriees
                .Select(h => new Refus(h.Strate, h.Domaine, Espece.Normatif, MotifLicenciableNonLicencie));
            return new EnsembleDesActes([], [.. refusContenu, .. refusDesStratesSuperieures]);
        }

        var elections = hypothesesTriees.Select(h => Elire(h, ce01)).ToList();
        return new EnsembleDesActes(elections, refusDesStratesSuperieures);
    }

    private static IReadOnlyList<Refus> RefusDesStratesSuperieures(IReadOnlyList<long> identifiantsDesActes)
    {
        var domaineMaximal = identifiantsDesActes.Distinct().OrderBy(id => id).ToList();

        // Ω vide : aucun acte d'observation, donc aucun domaine-strate à espace non trivial (014 C5 :
        // « pour chaque domaine-strate à espace non trivial, exactement un acte » ; 009 § 5, constat de
        // vacuité). Le domaine d'un acte de W est une énumération d'identifiants couverts (014 § 7.3) —
        // un refus sur domaine vide ne couvre rien et n'existe pas. W reste entier : simplement sans acte.
        if (domaineMaximal.Count == 0) return [];

        return
        [
            new Refus(Strate.Variante, domaineMaximal, Espece.Normatif, MotifAucuneConventionStrate),
            new Refus(Strate.Version, domaineMaximal, Espece.Normatif, MotifAucuneConventionStrate),
            new Refus(Strate.Identite, domaineMaximal, Espece.Normatif, MotifAucuneConventionStrate),
            new Refus(Strate.Famille, domaineMaximal, Espece.Normatif, MotifPrealableAbsent),
        ];
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
