using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Tests;

public class DecisionDesActesTests
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

    private static IReadOnlyList<Hypothese> HypothesesOracle() =>
        ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(ModeleOracle(), ReferentielReel()));

    private static IReadOnlyList<long> IdentifiantsOracle() =>
        ModeleOracle().Actes.Select(a => a.Identifiant).ToList();

    private static Convention Eq01 => new(
        "EQ-01", 1, Famille.Interpretation, "t", "t", [], "R1", "t", "t", "t", "t", "t", new DateOnly(2026, 7, 5), "t");

    private static string Cle(ActeElection e) =>
        $"E:{e.Strate}:{string.Join(",", e.Domaine)}:{e.ContenuPropositionnel}:{e.Niveau}:{e.Motif}:" +
        $"lic=[{string.Join(";", e.Licences)}]:dep=[{string.Join(";", e.Dependances)}]:dette=[{string.Join(";", e.Dette)}]";

    private static string Cle(Refus r) =>
        $"R:{r.Strate}:{string.Join(",", r.Domaine)}:{r.Espece}:{r.Motif}";

    private static IEnumerable<string> Cles(EnsembleDesActes actes) =>
        actes.Elections.Select(Cle).Concat(actes.Refus.Select(Cle));

    // --- aucune convention d'élection ⇒ uniquement des refus ---

    [Fact]
    public void Absence_de_CE01_ne_produit_que_des_refus_de_contenu_et_de_strates_superieures()
    {
        var type = new TypeDeSignal("contenu-identique", new ConventionRef("EQ-01", 1));
        IReadOnlyList<InstanceDeSignal> signaux =
        [
            new(type, "A", Regime.Exact, [new ObservationConsommee(1, "empreinte"), new ObservationConsommee(2, "empreinte")]),
        ];
        var hyps = ConstructionDesHypotheses.Construire(signaux);

        var actes = DecisionDesActes.Decider(hyps, new Referentiel([Eq01]), [1, 2]); // CE-01 absent

        Assert.Empty(actes.Elections);
        Assert.Equal(5, actes.Refus.Count); // 1 refus de contenu + 4 refus de strates supérieures

        var refusContenu = Assert.Single(actes.Refus, r => r.Strate == Strate.Contenu);
        Assert.Equal(Espece.Normatif, refusContenu.Espece);
        Assert.Equal("licenciable-non-licencié", refusContenu.Motif);
        Assert.Equal([1L, 2L], refusContenu.Domaine);

        foreach (var strate in new[] { Strate.Variante, Strate.Version, Strate.Identite })
        {
            var refus = Assert.Single(actes.Refus, r => r.Strate == strate);
            Assert.Equal(Espece.Normatif, refus.Espece);
            Assert.Equal("aucune-convention-strate", refus.Motif);
            Assert.Equal([1L, 2L], refus.Domaine);
        }

        var refusFamille = Assert.Single(actes.Refus, r => r.Strate == Strate.Famille);
        Assert.Equal(Espece.Normatif, refusFamille.Espece);
        Assert.Equal("préalable-absent", refusFamille.Motif);
    }

    // --- CE-01 élit exactement les hypothèses prévues, refuse les strates supérieures ---

    [Fact]
    public void CE01_elit_le_contenu_et_refuse_les_strates_superieures()
    {
        var hypotheses = HypothesesOracle();

        var actes = DecisionDesActes.Decider(hypotheses, ReferentielReel(), IdentifiantsOracle());

        Assert.Equal(4, actes.Refus.Count);
        Assert.Equal(112, actes.Elections.Count);
        Assert.Equal(108, actes.Elections.Count(e => e.Domaine.Count == 2));
        Assert.Equal(4, actes.Elections.Count(e => e.Domaine.Count == 3));
        Assert.All(actes.Elections, e =>
        {
            Assert.Equal(Strate.Contenu, e.Strate);
            Assert.Equal(Niveau.Certaine, e.Niveau);
            Assert.Equal("unique-maximale", e.Motif);
            Assert.Equal([new ConventionRef("CE-01", 1)], e.Licences);
            Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], e.Dependances);
            Assert.Empty(e.Dette);
        });

        Assert.All(actes.Refus, r => Assert.Equal(497, r.Domaine.Count));
        Assert.Equal(3, actes.Refus.Count(r => r.Motif == "aucune-convention-strate"));
        Assert.Single(actes.Refus, r => r.Motif == "préalable-absent" && r.Strate == Strate.Famille);
    }

    // --- déterminisme ---

    [Fact]
    public void Deux_decisions_sur_les_memes_entrees_produisent_le_meme_resultat()
    {
        var hypotheses = HypothesesOracle();
        var referentiel = ReferentielReel();
        var identifiants = IdentifiantsOracle();

        var premiere = DecisionDesActes.Decider(hypotheses, referentiel, identifiants);
        var seconde = DecisionDesActes.Decider(hypotheses, referentiel, identifiants);

        Assert.Equal(Cles(premiere), Cles(seconde));
    }

    // --- stabilité / reproductibilité entre deux exécutions indépendantes depuis l'oracle et le registre réel ---

    [Fact]
    public void Deux_executions_independantes_depuis_loracle_et_le_registre_produisent_le_meme_resultat()
    {
        var premiere = DecisionDesActes.Decider(HypothesesOracle(), ReferentielReel(), IdentifiantsOracle());
        var seconde = DecisionDesActes.Decider(HypothesesOracle(), ReferentielReel(), IdentifiantsOracle());

        Assert.Equal(Cles(premiere), Cles(seconde));
    }

    // --- refus explicites : un résultat positif, jamais une exception, jamais un silence ---

    [Fact]
    public void Un_refus_porte_son_espece_et_son_motif_sans_lever_dexception()
    {
        var hyps = ConstructionDesHypotheses.Construire(
        [
            new(new TypeDeSignal("contenu-identique", new ConventionRef("EQ-01", 1)), "X", Regime.Exact,
                [new ObservationConsommee(5, "empreinte"), new ObservationConsommee(6, "empreinte")]),
        ]);

        var actes = DecisionDesActes.Decider(hyps, new Referentiel([Eq01]), [5, 6]); // sans CE-01

        Assert.Empty(actes.Elections);
        Assert.Equal(5, actes.Refus.Count);
    }

    // --- indépendance de l'ordre d'entrée ---

    [Fact]
    public void Lordre_des_hypotheses_ne_change_pas_les_actes_produits()
    {
        var hypotheses = HypothesesOracle();
        var referentiel = ReferentielReel();
        var identifiants = IdentifiantsOracle();

        var direct = DecisionDesActes.Decider(hypotheses, referentiel, identifiants);
        var inverse = DecisionDesActes.Decider(hypotheses.Reverse().ToList(), referentiel, identifiants.Reverse().ToList());

        Assert.Equal(Cles(direct), Cles(inverse));
    }

    // --- indépendance de l'ordre des conventions (I31 étendu à ℛ, EXG-27) ---

    [Fact]
    public void Lordre_des_conventions_dans_le_referentiel_ne_change_pas_les_actes_produits()
    {
        var hypotheses = HypothesesOracle();
        var referentiel = ReferentielReel();
        var referentielInverse = new Referentiel(referentiel.ConventionsEnVigueur.Reverse().ToList());
        var identifiants = IdentifiantsOracle();

        var direct = DecisionDesActes.Decider(hypotheses, referentiel, identifiants);
        var inverse = DecisionDesActes.Decider(hypotheses, referentielInverse, identifiants);

        Assert.Equal(Cles(direct), Cles(inverse));
    }

    // --- reconstruction depuis Ω et ℛ (bout en bout) ---

    [Fact]
    public void Les_actes_se_reconstruisent_depuis_Omega_et_ℛ_via_le_pipeline_complet()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var actesParId = modele.Actes.ToDictionary(a => a.Identifiant);

        var actes = DecisionDesActes.Decider(
            ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel)),
            referentiel,
            modele.Actes.Select(a => a.Identifiant).ToList());

        Assert.Equal(112, actes.Elections.Count);
        Assert.All(actes.Elections, e =>
        {
            foreach (var acteId in e.Domaine)
            {
                Assert.Equal(e.ContenuPropositionnel, actesParId[acteId].Empreinte);
            }
        });
    }

    // --- B (audit final, A5) : Ω vide — aucun acte d'observation, aucun domaine-strate non trivial, aucun acte de W (014 C5, 009 § 5) ---

    [Fact]
    public void Omega_vide_ne_produit_aucun_acte()
    {
        var actes = DecisionDesActes.Decider([], ReferentielReel(), []);

        Assert.Empty(actes.Elections);
        Assert.Empty(actes.Refus);
    }

    // --- adaptateur mémoire identique à SQLite ---

    [Fact]
    public void Ladaptateur_memoire_produit_les_memes_actes_que_le_modele_direct()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var identifiants = modele.Actes.Select(a => a.Identifiant).ToList();
        var source = new SourceObservationsEnMemoire(modele, []);

        var directs = DecisionDesActes.Decider(
            ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel)),
            referentiel,
            identifiants);
        var viaAdaptateur = DecisionDesActes.Decider(
            ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(source.ProjeterModele(), referentiel)),
            referentiel,
            identifiants);

        Assert.Equal(Cles(directs), Cles(viaAdaptateur));
    }

    // --- V2-4 : l'application par famille (017 § 2, report 1) ---

    private static Convention ConventionElection(string identifiant) => new(
        identifiant, 1, Famille.Election, "t", "t", [], "R1", "t", "t", "t", "t", "t", new DateOnly(2026, 7, 5), "t");

    private static IReadOnlyList<Hypothese> HypotheseSimple() =>
        ConstructionDesHypotheses.Construire(
        [
            new(new TypeDeSignal("contenu-identique", new ConventionRef("EQ-01", 1)), "A", Regime.Exact,
                [new ObservationConsommee(1, "empreinte"), new ObservationConsommee(2, "empreinte")]),
        ]);

    [Fact]
    public void CE02_seule_licencie_les_elections()
    {
        var referentiel = new Referentiel([Eq01, ConventionElection("CE-02")]);

        var actes = DecisionDesActes.Decider(HypotheseSimple(), referentiel, [1, 2]);

        var election = Assert.Single(actes.Elections);
        Assert.Equal([new ConventionRef("CE-02", 1)], election.Licences);
        Assert.Equal([new ConventionRef("CE-02", 1), new ConventionRef("EQ-01", 1)], election.Dependances);
        Assert.Equal(Niveau.Certaine, election.Niveau);
        Assert.Equal("unique-maximale", election.Motif);
    }

    [Fact]
    public void Deux_conventions_delection_coexistent_et_licencient_ensemble()
    {
        // I27, 014 § 7.3 : l'élection cite la liste triée des licences — les deux conventions
        // d'élection en vigueur fondent ensemble le même acte, jamais deux actes.
        var referentiel = new Referentiel([Eq01, ConventionElection("CE-01"), ConventionElection("CE-02")]);

        var actes = DecisionDesActes.Decider(HypotheseSimple(), referentiel, [1, 2]);

        var election = Assert.Single(actes.Elections);
        Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("CE-02", 1)], election.Licences);
        Assert.Equal(
            [new ConventionRef("CE-01", 1), new ConventionRef("CE-02", 1), new ConventionRef("EQ-01", 1)],
            election.Dependances);
    }

    [Fact]
    public void Une_convention_nommee_CE01_dune_autre_famille_nest_jamais_utilisee()
    {
        var horsFamille = new Referentiel([Eq01, ConventionElection("CE-01") with { Famille = Famille.Interpretation }]);

        var actes = DecisionDesActes.Decider(HypotheseSimple(), horsFamille, [1, 2]);

        Assert.Empty(actes.Elections);
        var refusContenu = Assert.Single(actes.Refus, r => r.Strate == Strate.Contenu);
        Assert.Equal("licenciable-non-licencié", refusContenu.Motif);
    }

    [Fact]
    public void Une_convention_didentifiant_arbitraire_de_la_famille_election_est_appliquee()
    {
        var referentiel = new Referentiel([Eq01, ConventionElection("XX-99")]);

        var actes = DecisionDesActes.Decider(HypotheseSimple(), referentiel, [1, 2]);

        var election = Assert.Single(actes.Elections);
        Assert.Equal([new ConventionRef("XX-99", 1)], election.Licences);
    }

    [Fact]
    public void Les_refus_des_strates_superieures_derivent_du_registre_jamais_dune_liste_de_conventions()
    {
        // La dérivation est mécanique : aucune convention en vigueur ne fonde de signal ni de
        // licence aux strates supérieures (014 § 7.4) — le résultat est identique quel que soit
        // le contenu d'élection du registre, et dérive de sa consultation, pas d'une liste codée.
        var sansElection = DecisionDesActes.Decider([], new Referentiel([Eq01]), [1, 2]);
        var avecElections = DecisionDesActes.Decider(
            [], new Referentiel([Eq01, ConventionElection("CE-01"), ConventionElection("CE-02")]), [1, 2]);

        Assert.Equal(sansElection.Refus.Select(Cle), avecElections.Refus.Select(Cle));
        Assert.Equal(4, sansElection.Refus.Count);
        Assert.Equal(3, sansElection.Refus.Count(r => r.Motif == "aucune-convention-strate"));
        Assert.Single(sansElection.Refus, r => r.Strate == Strate.Famille && r.Motif == "préalable-absent");
    }

    [Fact]
    public void Apprentissage_sans_changement_de_moteur_un_registre_enrichi_de_CE02_est_applique_de_bout_en_bout()
    {
        // La démonstration symétrique d'EQ-02 (016 § 5.1, 017 § 2) : le registre AvecCe02
        // traverse C2 (couvert), C3 fonde les signaux, C4 construit, et C5 licencie par les
        // deux conventions d'élection — sans qu'une seule ligne du moteur ait changé.
        var cheminFixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "RegistresValides", "AvecCe02", "registre");
        var referentiel = new LecteurDeRegistreMarkdown(cheminFixture).Projeter();
        var modele = new ModeleObservations([
            new ActeObservation(1, 1, "A", new Dictionary<Attribut, ValeurObservee>()),
            new ActeObservation(2, 1, "A", new Dictionary<Attribut, ValeurObservee>()),
        ]);

        var actes = DecisionDesActes.Decider(
            ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel)),
            referentiel,
            [1, 2]);

        var election = Assert.Single(actes.Elections);
        Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("CE-02", 1)], election.Licences);
        Assert.Equal(
            [new ConventionRef("CE-01", 1), new ConventionRef("CE-02", 1), new ConventionRef("EQ-01", 1)],
            election.Dependances);
    }
}
