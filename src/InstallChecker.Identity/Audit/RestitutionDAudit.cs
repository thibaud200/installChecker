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
public static class RestitutionDAudit
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

    /// <summary>Résout l'acte désigné dans le W désigné — la seule clause de refus du contrat de C7 (014 C7).</summary>
    public static ActeW TrouverActeDesigne(W w, ReferenceActe reference) =>
        w.Actes.FirstOrDefault(a => a.Strate == reference.Strate && a.Domaine[0] == reference.PlusPetitIdentifiantDuDomaine)
        ?? throw new ActeInexistantDansWException($"acte inexistant dans le W désigné : {reference}");

    /// <summary>« Pourquoi cette élection ? » (011 § 7) : la chaîne complète observation → signal → hypothèse → élection → état.</summary>
    public static Chaine PourquoiCetteElection(ActeW acte, IReadOnlyList<Hypothese> hypotheses)
    {
        if (acte.Type != TypeActe.Election)
        {
            throw new InvalidOperationException("PourquoiCetteElection ne s'applique qu'à une élection");
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
            throw new InvalidOperationException("PourquoiCeRefus ne s'applique qu'à un refus");
        }

        if (!RuptureParMotif.TryGetValue(acte.Motif, out var rupture))
        {
            throw new InvalidOperationException($"motif de refus non reconnu par C7 : {acte.Motif}");
        }

        var hypothese = TrouverHypothese(acte, hypotheses);
        var maillons = new List<Maillon> { MaillonObservationsDuDomaine(acte, hypothese) };

        if (hypothese is not null)
        {
            var statutSignal = hypothese.Sig.Any(s => s.Regime != Regime.Exact) ? StatutMaillon.PorteRegime : StatutMaillon.Nominal;
            maillons.Add(new Maillon(Couche.C3, hypothese.Obs, hypothese.ConventionsMobilisees,
                $"{hypothese.Sig.Count} instance(s) de signal, sortie « {hypothese.ContenuPropositionnel} »", statutSignal));
            maillons.Add(new Maillon(Couche.C4, hypothese.Obs, [], $"hypothèse : {hypothese.Justification}", StatutMaillon.Nominal));
        }

        return new Chaine(maillons, rupture.Manque);
    }

    /// <summary>« De quelles conventions dépend cet acte ? » (011 § 7) : Dep et dette déjà portés par l'acte — jamais recalculés.</summary>
    public static DependancesReponse DeQuellesConventionsDependCetActe(ActeW acte) =>
        new(acte.Dependances ?? [], acte.Dette ?? []);

    /// <summary>« De quelles observations dépend-il ? » (011 § 7) : Obs trié, porté par l'hypothèse qui le fonde — absent si aucune n'existe.</summary>
    public static IReadOnlyList<ObservationConsommee> DeQuellesObservationsDependIl(ActeW acte, IReadOnlyList<Hypothese> hypotheses) =>
        TrouverHypothese(acte, hypotheses)?.Obs ?? [];

    /// <summary>
    /// « Qu'a-t-on écarté ? » (011 § 7) : les hypothèses concurrentes de même strate et de domaine
    /// recoupant, non retenues. Sous ℛ₀, C4 ne produit jamais deux hypothèses de domaines recoupants
    /// (chaque classe de contenu est un consensus dégénéré, 003 § 9) — cette liste est donc
    /// structurellement toujours vide ; elle ne compare rien elle-même (interdit à C7, 012 § 2), elle
    /// ne fait que constater l'absence de concurrent.
    /// </summary>
    public static IReadOnlyList<HypotheseEcartee> QuALonEcarte(ActeW acte, IReadOnlyList<Hypothese> hypotheses)
    {
        var retenue = TrouverHypothese(acte, hypotheses);

        return hypotheses
            .Where(h => h.Strate == acte.Strate && h.Domaine.Any(id => acte.Domaine.Contains(id)))
            .Where(h => retenue is null
                || !h.Domaine.SequenceEqual(retenue.Domaine)
                || h.ContenuPropositionnel != retenue.ContenuPropositionnel)
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

    private static Maillon MaillonObservationsDuDomaine(ActeW acte, Hypothese? hypothese) =>
        hypothese is not null
            ? new Maillon(Couche.C1, hypothese.Obs, [], $"{hypothese.Domaine.Count} acte(s) projeté(s)", StatutMaillon.Nominal)
            : new Maillon(Couche.C1, [], [], $"domaine maximal ({acte.Domaine.Count} acte(s) d'Ω), aucun attribut consulté à ce niveau (I51)", StatutMaillon.Nominal);
}
