using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.Identity.Actes;

/// <summary>
/// C5 — décision des actes (012 § 1.2, 014 C5). Fonction pure : (hypothèses de C4, référentiel de
/// C2) → ensemble des actes. Ne relit jamais les signaux ni les observations (012 § 3 : C5 décide
/// sur les objets d'hypothèses, qui portent déjà régimes et provenances) et ne calcule aucune
/// hypothèse — l'espace lui est donné par C4.
///
/// C5 applique la famille <b>élection</b> — jamais une convention par identifiant (017 § 2,
/// report 1 : « appliquer chaque convention selon sa famille ») : toute convention en vigueur de
/// cette famille licencie les élections, sans changement de moteur (016 § 5.1). L'espèce
/// d'élection appliquée est celle que la théorie définit pour la famille à ce jour (007 § 4,
/// instanciée au 009 § 4) : la configuration licenciée est l'hypothèse maximale structurellement
/// unique de strate contenu (003 § 9, consensus dégénéré), soutenue en régime exact — élue au
/// niveau « certaine », motif normalisé `unique-maximale` (014 § 7.4). Sous les registres
/// actuels, chaque hypothèse produite par C4 réalise cette configuration : aucune comparaison
/// entre hypothèses n'est jamais nécessaire ici (I25 : C5 ne fabrique aucun candidat, il
/// sélectionne). Une convention d'élection dont la portée sortirait de cette espèce serait une
/// forme que la théorie ne définit pas — révision documentaire d'abord (011 § 10), jamais une
/// latitude du moteur (EXG-13). Chaque élection cite en licences la totalité des conventions
/// d'élection en vigueur qui la fondent, triées (I27 : licences non vides ; 014 § 7.3).
///
/// En l'absence de toute convention d'élection en vigueur, aucune élection n'est de plein droit
/// (I27, 007 § 3) : chaque hypothèse formulable retombe en refus normatif — « configuration
/// licenciable, aucune convention adoptée » (motif normalisé `licenciable-non-licencié`, 014 § 7.4).
///
/// Les refus des strates supérieures dérivent du contenu de ℛ, jamais d'une liste codée : une
/// strate qu'aucune convention en vigueur ne fonde — ni signal ni licence (014 § 7.4) — est
/// refusée `aucune-convention-strate` ; la strate famille exige des identités déjà retenues, que
/// la strate identité, elle-même non fondée, ne retient jamais — refus `préalable-absent`.
/// Ces refus couvrent toujours le domaine maximal (la totalité des actes d'Ω, seule information
/// nécessaire ici — Obs(h) et le contenu des actes restent hors de portée de C5, I51).
/// </summary>
public static class DecisionDesActes
{
    private const string MotifUniqueMaximale = "unique-maximale";
    private const string MotifLicenciableNonLicencie = "licenciable-non-licencié";
    private const string MotifAucuneConventionStrate = "aucune-convention-strate";
    private const string MotifPrealableAbsent = "préalable-absent";

    public static EnsembleDesActes Decider(
        IReadOnlyList<Hypothese> hypotheses, Referentiel referentiel, IReadOnlyList<long> identifiantsDesActes)
    {
        var licences = referentiel.ConventionsEnVigueur
            .Where(c => c.Famille == Famille.Election)
            .OrderBy(c => c.Identifiant, StringComparer.Ordinal)
            .ThenBy(c => c.Version)
            .Select(c => c.Ref)
            .ToList();

        var hypothesesTriees = hypotheses.OrderBy(h => h.Domaine[0]).ToList();
        var refusDesStratesSuperieures = RefusDesStratesSuperieures(referentiel, identifiantsDesActes);

        if (licences.Count == 0)
        {
            var refusContenu = hypothesesTriees
                .Select(h => new Refus(h.Strate, h.Domaine, Espece.Normatif, MotifLicenciableNonLicencie));
            return new EnsembleDesActes([], [.. refusContenu, .. refusDesStratesSuperieures]);
        }

        var elections = hypothesesTriees.Select(h => Elire(h, licences)).ToList();
        return new EnsembleDesActes(elections, refusDesStratesSuperieures);
    }

    private static IReadOnlyList<Refus> RefusDesStratesSuperieures(
        Referentiel referentiel, IReadOnlyList<long> identifiantsDesActes)
    {
        var domaineMaximal = identifiantsDesActes.Distinct().OrderBy(id => id).ToList();

        // Ω vide : aucun acte d'observation, donc aucun domaine-strate à espace non trivial (014 C5 :
        // « pour chaque domaine-strate à espace non trivial, exactement un acte » ; 009 § 5, constat de
        // vacuité). Le domaine d'un acte de W est une énumération d'identifiants couverts (014 § 7.3) —
        // un refus sur domaine vide ne couvre rien et n'existe pas. W reste entier : simplement sans acte.
        if (domaineMaximal.Count == 0) return [];

        var refus = new List<Refus>();

        foreach (var strate in new[] { Strate.Variante, Strate.Version, Strate.Identite })
        {
            if (!referentiel.ConventionsEnVigueur.Any(c => FondeLaStrate(c, strate)))
            {
                refus.Add(new Refus(strate, domaineMaximal, Espece.Normatif, MotifAucuneConventionStrate));
            }
        }

        // La strate famille exige des identités déjà retenues (014 § 7.4 : « la famille sans
        // identités ») : la strate identité — que rien ne fonde dans ℛ (vérification ci-dessus) —
        // n'en retient aucune ; le préalable est absent, mécaniquement.
        refus.Add(new Refus(Strate.Famille, domaineMaximal, Espece.Normatif, MotifPrealableAbsent));

        return refus;
    }

    /// <summary>
    /// Vrai si la convention fonde un signal ou une licence à la strate donnée (014 § 7.4, motif
    /// `aucune-convention-strate`). À l'état actuel de la théorie, l'espèce de chaque famille
    /// couverte — interprétation : signaux de contenu (007 § 4) ; élection : licences de contenu
    /// (007 § 4, portée de CE-01) — ne fonde que la strate contenu : aucune convention ne peut
    /// fonder une strate supérieure sans révision théorique préalable (011 § 10 : théorie d'abord,
    /// moteur ensuite, revalidé — 016 § 5.1).
    /// </summary>
    private static bool FondeLaStrate(Convention convention, Strate strate) =>
        strate == Strate.Contenu && convention.Famille is Famille.Interpretation or Famille.Election;

    private static ActeElection Elire(Hypothese hypothese, IReadOnlyList<ConventionRef> licences)
    {
        var dependances = hypothese.ConventionsMobilisees
            .Concat(licences)
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
            Licences: licences,
            Dependances: dependances,
            Dette: []);
    }
}
