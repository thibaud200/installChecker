using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Audit;

/// <summary>
/// C7 — restitution d'audit (012 § 1.2, 014 C7). Lecture seule intégrale des objets déjà produits
/// par C4 (hypothèses) et C6 (W, τ) : elle ne crée, ne compare, ni ne recalcule jamais une hypothèse,
/// un signal, un acte, un refus, W ou τ — elle les restitue, maillon par maillon, sous la forme des
/// sept réponses contractuelles (011 § 7, 014 § 9). Toute réponse est une fonction déterministe de
/// (question, acte, index), re-dérivable à l'identique par un tiers depuis les mêmes objets (I39).
///
/// Sous ℛ₀, aucune contradiction ni aucune hypothèse concurrente n'existe jamais (consensus dégénéré,
/// 003 § 9) : les branches qui les concerneraient sont écrites pour rester correctes si ℛ change,
/// mais ne sont jamais empruntées sur les données actuelles — vérifié par les tests.
/// </summary>
internal static class RestitutionDAudit
{
    private static readonly IReadOnlyDictionary<string, (Couche Rupture, string Manque)> RuptureParMotif =
        new Dictionary<string, (Couche, string)>
        {
            ["aucune-convention-strate"] = (Couche.C3,
                "aucune convention en vigueur ne fonde de signal ni de licence à cette strate (014 § 7.4)"),
            ["préalable-absent"] = (Couche.C4,
                "la strate exige des rétentions préalables inexistantes (014 § 7.4)"),
            ["licenciable-non-licencié"] = (Couche.C5,
                "configuration licenciable, aucune convention d'élection en vigueur (007 § 3, I27)"),
        };

    /// <summary>
    /// Résout l'acte désigné dans le W désigné — la seule clause de refus du contrat de C7 (014 C7).
    /// La désignation est la forme abrégée (strate, plus petit identifiant du domaine), univoque sur
    /// tous les états dérivables sous la couverture de la version courante (024 § 3).
    /// </summary>
    public static ActeW TrouverActeDesigne(W w, Strate strate, long plusPetitIdentifiantDuDomaine) =>
        w.Actes.FirstOrDefault(a => a.Strate == strate && a.Domaine[0] == plusPetitIdentifiantDuDomaine)
        ?? throw new ActeInexistantDansWException(
            $"acte inexistant dans le W désigné : strate {strate}, plus petit identifiant {plusPetitIdentifiantDuDomaine}");

    /// <summary>« Pourquoi cette élection ? » (011 § 7) : la chaîne complète observation → signal → hypothèse → élection → état.</summary>
    public static Chaine PourquoiCetteElection(ActeW acte, IReadOnlyList<Hypothese> hypotheses)
    {
        if (acte.Type != TypeActe.Election)
        {
            // La question désigne une élection qui n'existe pas dans le W désigné (l'acte y est un
            // refus) : c'est exactement la seule clause de refus du contrat de C7 (014 C7) — jamais
            // une exception générique.
            throw new ActeInexistantDansWException(
                $"« pourquoi cette élection ? » : aucune élection (strate={acte.Strate}, domaine=[{string.Join(",", acte.Domaine)}]) dans le W désigné — l'acte y est un refus");
        }

        var hypothese = TrouverHypothese(acte, hypotheses)
            ?? throw new InvalidOperationException("aucune hypothèse ne fonde cette élection — W incohérent");

        var statutSignal = hypothese.Sig.Any(s => s.Regime != Regime.Exact) ? StatutMaillon.PorteRegime : StatutMaillon.Nominal;
        var statutElection = acte.Dette is { Count: > 0 } ? StatutMaillon.PorteDette : StatutMaillon.Nominal;

        return new Chaine(
        [
            new Maillon(Couche.C1, hypothese.Obs, [], $"{hypothese.Domaine.Count} acte(s) projeté(s)", StatutMaillon.Nominal),
            new Maillon(Couche.C3, hypothese.Obs, hypothese.ConventionsMobilisees,
                $"{hypothese.Sig.Count} instance(s) de signal, sortie « {hypothese.ContenuPropositionnel} »", statutSignal),
            new Maillon(Couche.C4, hypothese.Obs, [], $"hypothèse : {hypothese.Justification}", StatutMaillon.Nominal),
            new Maillon(Couche.C5, hypothese.Obs, acte.Licences!, $"élection : niveau={acte.Niveau}, motif={acte.Motif}", statutElection),
            new Maillon(Couche.C6, hypothese.Obs, [], $"acte de W : strate={acte.Strate}, domaine=[{string.Join(",", acte.Domaine)}]", StatutMaillon.Nominal),
        ],
        ManqueNomme: null);
    }

    /// <summary>« Pourquoi ce refus ? » (011 § 7) : la chaîne interrompue jusqu'au maillon manquant nommé.</summary>
    public static Chaine PourquoiCeRefus(ActeW acte, IReadOnlyList<Hypothese> hypotheses)
    {
        if (acte.Type != TypeActe.Refus)
        {
            // Symétrique de PourquoiCetteElection : la question désigne un refus inexistant dans le
            // W désigné (l'acte y est une élection) — la seule clause de refus du contrat (014 C7).
            throw new ActeInexistantDansWException(
                $"« pourquoi ce refus ? » : aucun refus (strate={acte.Strate}, domaine=[{string.Join(",", acte.Domaine)}]) dans le W désigné — l'acte y est une élection");
        }

        // Le motif d'un refus appartient au vocabulaire normalisé (014 § 7.4), extensible par les
        // documents futurs : un motif que cette table n'enrichit pas encore est restitué tel quel —
        // il EST « le motif exact » que le contrat d'audit exige (011 § 7) — jamais une exception.
        var manque = RuptureParMotif.TryGetValue(acte.Motif, out var rupture) ? rupture.Manque : acte.Motif;

        var hypothesesDuRefus = TrouverHypothesesDuRefus(acte, hypotheses);
        var maillons = new List<Maillon> { MaillonObservationsDuDomaine(acte, hypothesesDuRefus) };

        foreach (var hypothese in hypothesesDuRefus)
        {
            var statutSignal = hypothese.Sig.Any(s => s.Regime != Regime.Exact) ? StatutMaillon.PorteRegime : StatutMaillon.Nominal;
            maillons.Add(new Maillon(Couche.C3, hypothese.Obs, hypothese.ConventionsMobilisees,
                $"{hypothese.Sig.Count} instance(s) de signal, sortie « {hypothese.ContenuPropositionnel} »", statutSignal));
            maillons.Add(new Maillon(Couche.C4, hypothese.Obs, [], $"hypothèse : {hypothese.Justification}", StatutMaillon.Nominal));
        }

        return new Chaine(maillons, manque);
    }

    /// <summary>« De quelles conventions dépend cet acte ? » (011 § 7) : Dep et dette déjà portés par l'acte — jamais recalculés.</summary>
    public static DependancesReponse DeQuellesConventionsDependCetActe(ActeW acte) =>
        new(acte.Dependances ?? [], acte.Dette ?? []);

    /// <summary>
    /// « De quelles observations dépend-il ? » (011 § 7) : Obs trié, porté par les hypothèses qui le
    /// fondent — absent si aucune n'existe. La réponse est une fonction de (acte, index) (014 § 9) :
    /// pour un refus, elle est identique que C6 ait agrégé ou non les refus élémentaires (l'agrégation
    /// est une mise en forme, 014 § 7.3) — l'union des Obs des hypothèses couvertes.
    /// </summary>
    public static IReadOnlyList<ObservationConsommee> DeQuellesObservationsDependIl(ActeW acte, IReadOnlyList<Hypothese> hypotheses) =>
        acte.Type == TypeActe.Election
            ? TrouverHypothese(acte, hypotheses)?.Obs ?? []
            : ObsDe(TrouverHypothesesDuRefus(acte, hypotheses));

    /// <summary>
    /// « Qu'a-t-on écarté ? » (011 § 7) : les hypothèses concurrentes de même strate et de domaine
    /// recoupant, non retenues. Sous ℛ₀, C4 ne produit jamais deux hypothèses de domaines recoupants
    /// (chaque classe de contenu est un consensus dégénéré, 003 § 9) — cette liste est donc
    /// structurellement toujours vide ; elle ne compare rien elle-même (interdit à C7, 012 § 2), elle
    /// ne fait que constater l'absence de concurrent.
    /// </summary>
    public static IReadOnlyList<HypotheseEcartee> QuALonEcarte(ActeW acte, IReadOnlyList<Hypothese> hypotheses)
    {
        // Les hypothèses propres à l'acte ne sont jamais des « écartées » (014 § 9 : écartée =
        // concurrente dominée ou incomparable-non-licenciée) : l'élue pour une élection, les
        // hypothèses refusées elles-mêmes pour un refus — élémentaire ou agrégé, l'agrégation de C6
        // étant une mise en forme qui ne change pas ce qui a été écarté (014 § 7.3).
        IReadOnlyList<Hypothese> propres = acte.Type == TypeActe.Election
            ? TrouverHypothese(acte, hypotheses) is { } retenue ? [retenue] : []
            : TrouverHypothesesDuRefus(acte, hypotheses);

        return hypotheses
            .Where(h => h.Strate == acte.Strate && h.Domaine.Any(id => acte.Domaine.Contains(id)))
            .Where(h => !propres.Contains(h))
            .Select(h => new HypotheseEcartee(h.ContenuPropositionnel,
                "non retenue : hors du consensus dégénéré (003 § 9) — aucune comparaison n'a eu lieu"))
            .ToList();
    }

    /// <summary>
    /// « Que faudrait-il renier pour que ceci tombe ? » (011 § 7, 008 § 8) : sous ℛ₀, aucun acte n'a
    /// de soutien alternatif (008 § 8 « non-unicité possible » ne se réalise jamais ici — une seule
    /// convention par famille active) : l'unique ensemble minimal est Dep lui-même.
    /// </summary>
    public static EnsemblesMinimauxReponse QueFaudraitIlRenierPourQueCeciTombe(ActeW acte) =>
        new([acte.Dependances ?? []], acte.Dette ?? []);

    /// <summary>« Qu'est-ce qui a changé entre deux états ? » (011 § 7) : τ, déjà produit par C6 — restitué tel quel, jamais recalculé.</summary>
    public static Transition QuEstCeQuiAChangeEntreDeuxEtats(Transition tau) => tau;

    private static Hypothese? TrouverHypothese(ActeW acte, IReadOnlyList<Hypothese> hypotheses) =>
        hypotheses.SingleOrDefault(h => h.Strate == acte.Strate
            && h.Domaine.SequenceEqual(acte.Domaine)
            && (acte.Type != TypeActe.Election || h.ContenuPropositionnel == acte.Contenu));

    /// <summary>
    /// Les hypothèses réellement produites par C4 que le refus couvre. Après l'agrégation canonique
    /// des refus (014 § 7.3), le domaine de l'acte est l'union des domaines des refus fusionnés :
    /// chaque hypothèse refusée s'y retrouve par inclusion de son domaine — jamais recalculée, jamais
    /// re-dérivée (lecture seule, 012 § 2). Un refus non agrégé retombe sur l'égalité exacte (cas
    /// particulier de l'inclusion) ; un refus sans hypothèse (strates supérieures) donne la liste vide.
    /// </summary>
    private static IReadOnlyList<Hypothese> TrouverHypothesesDuRefus(ActeW acte, IReadOnlyList<Hypothese> hypotheses)
    {
        var domaine = acte.Domaine.ToHashSet();

        return hypotheses
            .Where(h => h.Strate == acte.Strate && h.Domaine.All(domaine.Contains))
            .OrderBy(h => h.Domaine[0])
            .ToList();
    }

    private static Maillon MaillonObservationsDuDomaine(ActeW acte, IReadOnlyList<Hypothese> hypothesesDuRefus)
    {
        if (hypothesesDuRefus.Count == 0)
        {
            return new Maillon(Couche.C1, [], [], $"domaine maximal ({acte.Domaine.Count} acte(s) d'Ω), aucun attribut consulté à ce niveau (I51)", StatutMaillon.Nominal);
        }

        var nombreActes = hypothesesDuRefus.SelectMany(h => h.Domaine).Distinct().Count();

        return new Maillon(Couche.C1, ObsDe(hypothesesDuRefus), [], $"{nombreActes} acte(s) projeté(s)", StatutMaillon.Nominal);
    }

    /// <summary>L'union triée des Obs d'un ensemble d'hypothèses — le tri de l'Obs (014 § 9), identique à celui de C4.</summary>
    private static IReadOnlyList<ObservationConsommee> ObsDe(IReadOnlyList<Hypothese> hypotheses) =>
        hypotheses
            .SelectMany(h => h.Obs)
            .Distinct()
            .OrderBy(o => o.ActeId)
            .ThenBy(o => o.Attribut, StringComparer.Ordinal)
            .ToList();
}
