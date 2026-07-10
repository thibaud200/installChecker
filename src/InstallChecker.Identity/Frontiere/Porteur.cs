using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Audit;
using InstallChecker.Identity.Auxiliaire;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Frontiere;

/// <summary>
/// Le porteur de la fonction (Ω, ℛ) → W (018 § 2, report 2) : la réalisation d'EXG-01. Il compose
/// les couches C1→C6 derrière la frontière du 011 et expose les trois invocations contractuelles —
/// dérivation, transition, audit (cette dernière servie par C7, derrière la même frontière).
/// Il reçoit exactement les deux ports (013 § 1.1) et rien d'autre : aucune option, aucun réglage
/// (EXG-02, 011 § 2.2 — « il n'existe aucune troisième entrée »).
///
/// Il ne dérive rien et ne vérifie rien en propre (018 § 2, I66) : les vérifications d'Ω
/// appartiennent à C1, celles de ℛ — couverture comprise — à C2 (017 § 5) ; en aval, tout objet
/// consommé est valide et couvert par construction (I51 étendu). Ce qu'il possède en propre est
/// exactement double : l'<b>ordre d'invocation</b> — le bloc Ω d'abord (absent &lt; incompatible
/// &lt; invalide, dans C1), puis le bloc ℛ (absence &lt; forme &lt; cohérence &lt; couverture,
/// dans C2) : l'ordre total du 018 § 4, dont le signalement du premier échec est déterministe
/// (I67) — et la garantie « <b>entier ou absent</b> » à la frontière publique (011 § 4) : toute
/// erreur interrompt l'invocation avant toute émission ; aucun W, τ ni réponse partiels n'existent.
///
/// Les erreurs des couches sont surfacées telles quelles — jamais renommées, jamais agrégées,
/// jamais converties (018 § 3). Les réponses d'audit sont re-dérivées de l'index à chaque
/// invocation (011 § 7 : « dérivée du seul index » ; I39 ; 013 § 4 : le moteur recalcule
/// toujours). Pour la transition, la cause est transportée telle quelle, sans vérification —
/// forme du 014 § 7.5, fourniture par l'appelant sous le régime actuel (016 § 4.2, report 9).
/// </summary>
public static class Porteur
{
    // --- L'invocation de dérivation (011 § 3 : W) ---

    public static W Deriver(IObservationsSource omega, IRegistreSource registre) =>
        DeriverAvecHypotheses(omega, registre).W;

    // --- L'invocation de transition (011 § 3 : τ — deux index dont le porteur reçoit les deux membres) ---

    public static Transition Transitionner(
        IObservationsSource omegaAvant, IRegistreSource registreAvant,
        IObservationsSource omegaApres, IRegistreSource registreApres,
        Cause cause)
    {
        // Membre par membre, dans l'ordre des sections du 014 § 7.5 : l'index avant, puis l'index après (018 § 4).
        var avant = Deriver(omegaAvant, registreAvant);
        var apres = Deriver(omegaApres, registreApres);
        return AssemblageDeLetat.CalculerTransition(avant, apres, cause);
    }

    // --- L'invocation d'audit (011 § 7) : les questions de C7, sur un acte désigné d'un index désigné,
    //     re-dérivées à chaque invocation — le porteur route, C7 répond (surface identique à 014 § 1, C7) ---

    public static Chaine PourquoiCetteElection(IObservationsSource omega, IRegistreSource registre, Strate strate, long plusPetitIdentifiantDuDomaine)
    {
        var (w, hypotheses) = DeriverAvecHypotheses(omega, registre);
        return RestitutionDAudit.PourquoiCetteElection(RestitutionDAudit.TrouverActeDesigne(w, strate, plusPetitIdentifiantDuDomaine), hypotheses);
    }

    public static Chaine PourquoiCeRefus(IObservationsSource omega, IRegistreSource registre, Strate strate, long plusPetitIdentifiantDuDomaine)
    {
        var (w, hypotheses) = DeriverAvecHypotheses(omega, registre);
        return RestitutionDAudit.PourquoiCeRefus(RestitutionDAudit.TrouverActeDesigne(w, strate, plusPetitIdentifiantDuDomaine), hypotheses);
    }

    public static DependancesReponse DeQuellesConventionsDependCetActe(
        IObservationsSource omega, IRegistreSource registre, Strate strate, long plusPetitIdentifiantDuDomaine)
    {
        var (w, _) = DeriverAvecHypotheses(omega, registre);
        return RestitutionDAudit.DeQuellesConventionsDependCetActe(RestitutionDAudit.TrouverActeDesigne(w, strate, plusPetitIdentifiantDuDomaine));
    }

    public static IReadOnlyList<ObservationConsommee> DeQuellesObservationsDependIl(
        IObservationsSource omega, IRegistreSource registre, Strate strate, long plusPetitIdentifiantDuDomaine)
    {
        var (w, hypotheses) = DeriverAvecHypotheses(omega, registre);
        return RestitutionDAudit.DeQuellesObservationsDependIl(RestitutionDAudit.TrouverActeDesigne(w, strate, plusPetitIdentifiantDuDomaine), hypotheses);
    }

    public static IReadOnlyList<HypotheseEcartee> QuALonEcarte(
        IObservationsSource omega, IRegistreSource registre, Strate strate, long plusPetitIdentifiantDuDomaine)
    {
        var (w, hypotheses) = DeriverAvecHypotheses(omega, registre);
        return RestitutionDAudit.QuALonEcarte(RestitutionDAudit.TrouverActeDesigne(w, strate, plusPetitIdentifiantDuDomaine), hypotheses);
    }

    public static EnsemblesMinimauxReponse QueFaudraitIlRenierPourQueCeciTombe(
        IObservationsSource omega, IRegistreSource registre, Strate strate, long plusPetitIdentifiantDuDomaine)
    {
        var (w, _) = DeriverAvecHypotheses(omega, registre);
        return RestitutionDAudit.QueFaudraitIlRenierPourQueCeciTombe(RestitutionDAudit.TrouverActeDesigne(w, strate, plusPetitIdentifiantDuDomaine));
    }

    // --- La composition (018 § 5) : chaque traversée inter-couches est une ligne de la table du
    //     014 § 3 ; l'index est fourni à C6 au titre de sa clause « reçoit » (014 § 1), l'identité
    //     d'Ω selon le régime actuel du 014 § 7.2 (report 5 : convoyée, jamais définie ici) ---

    private static (W W, IReadOnlyList<Hypothese> Hypotheses) DeriverAvecHypotheses(
        IObservationsSource omega, IRegistreSource registre)
    {
        // Bloc Ω d'abord, bloc ℛ ensuite (018 § 4) — les deux entièrement vérifiés avant toute dérivation (017 § 4).
        var modele = omega.ProjeterModele();
        var referentiel = registre.Projeter();

        var signaux = DerivationDesSignaux.Deriver(modele, referentiel);
        var hypotheses = ConstructionDesHypotheses.Construire(signaux);
        var actes = DecisionDesActes.Decider(
            hypotheses, referentiel, modele.Actes.Select(a => a.Identifiant).ToList());
        var index = new IndexEtat(IndexOmegaCalculateur.Calculer(modele), referentiel.Index);

        return (AssemblageDeLetat.Assembler(actes, index), hypotheses);
    }
}
