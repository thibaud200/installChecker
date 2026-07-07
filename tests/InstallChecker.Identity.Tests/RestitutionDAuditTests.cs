using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Audit;
using InstallChecker.Identity.Auxiliaire;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Tests;

public class RestitutionDAuditTests
{
    private static string RacineDuDepot()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        return repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable");
    }

    private static Referentiel ReferentielReel() =>
        new LecteurDeRegistreMarkdown(Path.Combine(RacineDuDepot(), "registre")).Projeter();

    private static ModeleObservations ModeleOracle() =>
        new LecteurDObservationsSqlite(Path.Combine(RacineDuDepot(), "tests", "oracle", "corpus1-postA1.db")).ProjeterModele();

    private static IReadOnlyList<Hypothese> HypothesesOracle(ModeleObservations modele, Referentiel referentiel) =>
        ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel));

    private static W AssemblerW0(ModeleObservations modele, Referentiel referentiel, IReadOnlyList<Hypothese> hypotheses)
    {
        var identifiants = modele.Actes.Select(a => a.Identifiant).ToList();
        var actes = DecisionDesActes.Decider(hypotheses, referentiel, identifiants);
        var index = new IndexEtat(IndexOmegaCalculateur.Calculer(modele), referentiel.Index);
        return AssemblageDeLetat.Assembler(actes, index);
    }

    private static string Cle(Maillon m) =>
        $"{m.Couche}:obs=[{string.Join(";", m.Observations)}]:conv=[{string.Join(";", m.Conventions)}]:{m.ObjetProduit}:{m.Statut}";

    private static string Cle(Chaine c) =>
        $"manque={c.ManqueNomme}|" + string.Join("||", c.Maillons.Select(Cle));

    // --- résolution de l'acte désigné (014 C7 : seule clause de refus) ---

    [Fact]
    public void TrouverActeDesigne_retrouve_une_election_de_W0()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);

        var premiereElection = w.Actes.First(a => a.Type == TypeActe.Election);
        var reference = new ReferenceActe(premiereElection.Strate, premiereElection.Domaine[0]);

        var trouve = RestitutionDAudit.TrouverActeDesigne(w, reference);

        Assert.Same(premiereElection, trouve);
    }

    [Fact]
    public void TrouverActeDesigne_leve_lerreur_nommee_pour_un_acte_absent()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);

        Assert.Throws<ActeInexistantDansWException>(() =>
            RestitutionDAudit.TrouverActeDesigne(w, new ReferenceActe(Strate.Contenu, 999_999)));
    }

    // --- pourquoi cette élection : chaîne aboutie sur les 112 élections de W0 ---

    [Fact]
    public void PourquoiCetteElection_produit_une_chaine_aboutie_pour_chaque_election_de_W0()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);

        foreach (var election in w.Actes.Where(a => a.Type == TypeActe.Election))
        {
            var chaine = RestitutionDAudit.PourquoiCetteElection(election, hypotheses);

            Assert.Null(chaine.ManqueNomme);
            Assert.Equal(5, chaine.Maillons.Count);
            Assert.Equal([Couche.C1, Couche.C3, Couche.C4, Couche.C5, Couche.C6], chaine.Maillons.Select(m => m.Couche));
            Assert.All(chaine.Maillons, m => Assert.Equal(StatutMaillon.Nominal, m.Statut));

            var maillonSignal = chaine.Maillons.Single(m => m.Couche == Couche.C3);
            Assert.Equal([new ConventionRef("EQ-01", 1)], maillonSignal.Conventions);

            var maillonElection = chaine.Maillons.Single(m => m.Couche == Couche.C5);
            Assert.Equal(election.Licences, maillonElection.Conventions);
        }
    }

    [Fact]
    public void PourquoiCetteElection_leve_une_exception_sur_un_refus()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var refus = w.Actes.First(a => a.Type == TypeActe.Refus);

        Assert.Throws<InvalidOperationException>(() => RestitutionDAudit.PourquoiCetteElection(refus, hypotheses));
    }

    // --- pourquoi ce refus : chaîne interrompue sur les 4 refus de W0 ---

    [Fact]
    public void PourquoiCeRefus_nomme_le_manque_exact_pour_les_quatre_refus_de_W0()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);

        foreach (var strate in new[] { Strate.Variante, Strate.Version, Strate.Identite })
        {
            var refus = w.Actes.Single(a => a.Type == TypeActe.Refus && a.Strate == strate);
            var chaine = RestitutionDAudit.PourquoiCeRefus(refus, hypotheses);

            Assert.NotNull(chaine.ManqueNomme);
            Assert.Contains("aucune convention en vigueur", chaine.ManqueNomme);
            Assert.Single(chaine.Maillons);
            Assert.Equal(Couche.C1, chaine.Maillons[0].Couche);
            Assert.Empty(chaine.Maillons[0].Observations);
        }

        var refusFamille = w.Actes.Single(a => a.Type == TypeActe.Refus && a.Strate == Strate.Famille);
        var chaineFamille = RestitutionDAudit.PourquoiCeRefus(refusFamille, hypotheses);
        Assert.Contains("rétentions préalables", chaineFamille.ManqueNomme);
        Assert.Single(chaineFamille.Maillons);
    }

    [Fact]
    public void PourquoiCeRefus_reconstruit_la_chaine_jusquau_signal_quand_lhypothese_existe_sans_licence()
    {
        var type = new TypeDeSignal("contenu-identique", new ConventionRef("EQ-01", 1));
        IReadOnlyList<InstanceDeSignal> signaux =
        [
            new(type, "A", Regime.Exact, [new ObservationConsommee(1, "empreinte"), new ObservationConsommee(2, "empreinte")]),
        ];
        var hypotheses = ConstructionDesHypotheses.Construire(signaux);
        var eq01 = new Convention("EQ-01", 1, Famille.Interpretation, "t", "t", [], "R1", "t", "t", "t", "t", "t", new DateOnly(2026, 7, 5), "t");
        var actes = DecisionDesActes.Decider(hypotheses, new Referentiel([eq01]), [1, 2]); // CE-01 absent
        var refusContenu = ActeW.DepuisRefus(actes.Refus.Single(r => r.Strate == Strate.Contenu));

        var chaine = RestitutionDAudit.PourquoiCeRefus(refusContenu, hypotheses);

        Assert.Equal("configuration licenciable, aucune convention d'élection en vigueur (007 § 3, I27)", chaine.ManqueNomme);
        Assert.Equal([Couche.C1, Couche.C3, Couche.C4], chaine.Maillons.Select(m => m.Couche));
        Assert.Equal([new ObservationConsommee(1, "empreinte"), new ObservationConsommee(2, "empreinte")], chaine.Maillons[0].Observations);
    }

    [Fact]
    public void PourquoiCeRefus_leve_une_exception_sur_une_election()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var election = w.Actes.First(a => a.Type == TypeActe.Election);

        Assert.Throws<InvalidOperationException>(() => RestitutionDAudit.PourquoiCeRefus(election, hypotheses));
    }

    // --- de quelles conventions dépend cet acte ---

    [Fact]
    public void DeQuellesConventionsDependCetActe_restitue_Dep_et_dette_dune_election()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var election = w.Actes.First(a => a.Type == TypeActe.Election);

        var reponse = RestitutionDAudit.DeQuellesConventionsDependCetActe(election);

        Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], reponse.Dependances);
        Assert.Empty(reponse.Dette);
    }

    [Fact]
    public void DeQuellesConventionsDependCetActe_est_vide_pour_un_refus()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var refus = w.Actes.First(a => a.Type == TypeActe.Refus);

        var reponse = RestitutionDAudit.DeQuellesConventionsDependCetActe(refus);

        Assert.Empty(reponse.Dependances);
        Assert.Empty(reponse.Dette);
    }

    // --- de quelles observations dépend-il ---

    [Fact]
    public void DeQuellesObservationsDependIl_restitue_lObs_trie_dune_election()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var election = w.Actes.First(a => a.Type == TypeActe.Election);
        var hypothese = hypotheses.Single(h => h.ContenuPropositionnel == election.Contenu && h.Domaine.SequenceEqual(election.Domaine));

        var obs = RestitutionDAudit.DeQuellesObservationsDependIl(election, hypotheses);

        Assert.Equal(hypothese.Obs, obs);
        Assert.Equal(obs.OrderBy(o => o.ActeId).ThenBy(o => o.Attribut, StringComparer.Ordinal), obs);
    }

    [Fact]
    public void DeQuellesObservationsDependIl_est_vide_pour_un_refus_structurel()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var refus = w.Actes.First(a => a.Type == TypeActe.Refus);

        Assert.Empty(RestitutionDAudit.DeQuellesObservationsDependIl(refus, hypotheses));
    }

    // --- qu'a-t-on écarté : toujours vide sous ℛ₀ (consensus dégénéré, 003 § 9) ---

    [Fact]
    public void QuALonEcarte_est_toujours_vide_sur_W0()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);

        Assert.All(w.Actes, acte => Assert.Empty(RestitutionDAudit.QuALonEcarte(acte, hypotheses)));
    }

    // --- que faudrait-il renier ---

    [Fact]
    public void QueFaudraitIlRenier_restitue_un_unique_ensemble_minimal_egal_a_Dep_pour_une_election()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var election = w.Actes.First(a => a.Type == TypeActe.Election);

        var reponse = RestitutionDAudit.QueFaudraitIlRenierPourQueCeciTombe(election);

        Assert.Equal([election.Dependances!], reponse.EnsemblesMinimaux);
        Assert.Empty(reponse.Dette);
    }

    [Fact]
    public void QueFaudraitIlRenier_restitue_lensemble_vide_pour_un_refus()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var refus = w.Actes.First(a => a.Type == TypeActe.Refus);

        var reponse = RestitutionDAudit.QueFaudraitIlRenierPourQueCeciTombe(refus);

        Assert.Equal([Array.Empty<ConventionRef>()], reponse.EnsemblesMinimaux);
        Assert.Empty(reponse.Dette);
    }

    // --- qu'est-ce qui a changé : pur passe-plat de τ ---

    [Fact]
    public void QuEstCeQuiAChange_restitue_tau_sans_le_modifier()
    {
        var indexAvant = new IndexEtat(new IndexOmega(1, 3, "avant"), [new ConventionRef("CE-01", 1)]);
        var indexApres = new IndexEtat(new IndexOmega(1, 5, "apres"), [new ConventionRef("CE-01", 1)]);
        var tau = new Transition(indexAvant, indexApres, new Cause(TypeCause.Omega, "test"),
            new Correspondance([], [], [], []));

        var reponse = RestitutionDAudit.QuEstCeQuiAChangeEntreDeuxEtats(tau);

        Assert.Same(tau, reponse);
    }

    // --- le contrat d'audit honoré sur chaque acte de W0, pas seulement des échantillons (011 § 8.3, EXG-05) ---

    [Fact]
    public void Le_contrat_daudit_repond_aux_cinq_questions_par_acte_sur_les_116_actes_de_W0()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);

        Assert.Equal(116, w.Actes.Count);

        foreach (var acte in w.Actes)
        {
            var chaine = acte.Type == TypeActe.Election
                ? RestitutionDAudit.PourquoiCetteElection(acte, hypotheses)
                : RestitutionDAudit.PourquoiCeRefus(acte, hypotheses);
            Assert.NotEmpty(chaine.Maillons);
            Assert.Equal(acte.Type == TypeActe.Election, chaine.ManqueNomme is null);

            RestitutionDAudit.DeQuellesConventionsDependCetActe(acte);
            RestitutionDAudit.DeQuellesObservationsDependIl(acte, hypotheses);
            Assert.Empty(RestitutionDAudit.QuALonEcarte(acte, hypotheses)); // consensus dégénéré (003 § 9)
            Assert.Single(RestitutionDAudit.QueFaudraitIlRenierPourQueCeciTombe(acte).EnsemblesMinimaux);
        }
    }

    // --- déterminisme, stabilité, indépendance de l'ordre ---

    [Fact]
    public void Deux_restitutions_sur_les_memes_entrees_produisent_la_meme_chaine()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var election = w.Actes.First(a => a.Type == TypeActe.Election);
        var refus = w.Actes.First(a => a.Type == TypeActe.Refus);

        Assert.Equal(Cle(RestitutionDAudit.PourquoiCetteElection(election, hypotheses)), Cle(RestitutionDAudit.PourquoiCetteElection(election, hypotheses)));
        Assert.Equal(Cle(RestitutionDAudit.PourquoiCeRefus(refus, hypotheses)), Cle(RestitutionDAudit.PourquoiCeRefus(refus, hypotheses)));
    }

    [Fact]
    public void Lordre_des_hypotheses_en_entree_ne_change_pas_la_chaine_produite()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var election = w.Actes.First(a => a.Type == TypeActe.Election);

        var direct = RestitutionDAudit.PourquoiCetteElection(election, hypotheses);
        var inverse = RestitutionDAudit.PourquoiCetteElection(election, hypotheses.Reverse().ToList());

        Assert.Equal(Cle(direct), Cle(inverse));
    }

    // --- reconstruction depuis Ω et ℛ via le pipeline complet ---

    [Fact]
    public void Les_chaines_se_reconstruisent_depuis_Omega_et_ℛ_via_le_pipeline_complet()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel));
        var w = AssemblerW0(modele, referentiel, hypotheses);

        Assert.All(w.Actes, acte =>
        {
            var chaine = acte.Type == TypeActe.Election
                ? RestitutionDAudit.PourquoiCetteElection(acte, hypotheses)
                : RestitutionDAudit.PourquoiCeRefus(acte, hypotheses);

            Assert.NotEmpty(chaine.Maillons);
        });
    }

    // --- adaptateur mémoire identique à SQLite ---

    [Fact]
    public void Ladaptateur_memoire_produit_les_memes_chaines_que_le_modele_direct()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var source = new SourceObservationsEnMemoire(modele, []);

        var hypothesesDirectes = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel));
        var hypothesesViaAdaptateur = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(source.ProjeterModele(), referentiel));

        var wDirect = AssemblerW0(modele, referentiel, hypothesesDirectes);
        var electionDirecte = wDirect.Actes.First(a => a.Type == TypeActe.Election);

        var chaineDirecte = RestitutionDAudit.PourquoiCetteElection(electionDirecte, hypothesesDirectes);
        var chaineViaAdaptateur = RestitutionDAudit.PourquoiCetteElection(electionDirecte, hypothesesViaAdaptateur);

        Assert.Equal(Cle(chaineDirecte), Cle(chaineViaAdaptateur));
    }

    // --- aucune information ni décision supplémentaire : les sept réponses ne portent que des champs déjà produits ---

    [Fact]
    public void Aucune_reponse_daudit_ne_modifie_les_objets_quelle_restitue()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = HypothesesOracle(modele, referentiel);
        var w = AssemblerW0(modele, referentiel, hypotheses);
        var hypothesesAvant = hypotheses.Select(h => h.ContenuPropositionnel).ToList();
        var wAvant = w.Actes.Count;

        foreach (var acte in w.Actes)
        {
            _ = acte.Type == TypeActe.Election
                ? RestitutionDAudit.PourquoiCetteElection(acte, hypotheses)
                : RestitutionDAudit.PourquoiCeRefus(acte, hypotheses);
            _ = RestitutionDAudit.DeQuellesConventionsDependCetActe(acte);
            _ = RestitutionDAudit.DeQuellesObservationsDependIl(acte, hypotheses);
            _ = RestitutionDAudit.QuALonEcarte(acte, hypotheses);
            _ = RestitutionDAudit.QueFaudraitIlRenierPourQueCeciTombe(acte);
        }

        Assert.Equal(hypothesesAvant, hypotheses.Select(h => h.ContenuPropositionnel));
        Assert.Equal(wAvant, w.Actes.Count);
    }
}
