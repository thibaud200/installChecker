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

    private static Convention Eq01 => new(
        "EQ-01", 1, Famille.Interpretation, "t", "t", [], "R1", "t", "t", "t", "t", "t", new DateOnly(2026, 7, 5), "t");

    private static Convention Ce01 => new(
        "CE-01", 1, Famille.Election, "t", "t", [new ConventionRef("EQ-01", 1)], "R1", "t", "t", "t", "t", "t", new DateOnly(2026, 7, 5), "t");

    private static string Cle(ActeElection e) =>
        $"E:{e.Strate}:{string.Join(",", e.Domaine)}:{e.ContenuPropositionnel}:{e.Niveau}:{e.Motif}:" +
        $"lic=[{string.Join(";", e.Licences)}]:dep=[{string.Join(";", e.Dependances)}]:dette=[{string.Join(";", e.Dette)}]";

    private static string Cle(Refus r) =>
        $"R:{r.Strate}:{string.Join(",", r.Domaine)}:{r.Espece}:{r.Motif}";

    private static IEnumerable<string> Cles(EnsembleDesActes actes) =>
        actes.Elections.Select(Cle).Concat(actes.Refus.Select(Cle));

    // --- aucune convention d'élection ⇒ uniquement des refus ---

    [Fact]
    public void Absence_de_CE01_ne_produit_que_des_refus()
    {
        var type = new TypeDeSignal("contenu-identique", new ConventionRef("EQ-01", 1));
        IReadOnlyList<InstanceDeSignal> signaux =
        [
            new(type, "A", Regime.Exact, [new ObservationConsommee(1, "empreinte"), new ObservationConsommee(2, "empreinte")]),
        ];
        var hyps = ConstructionDesHypotheses.Construire(signaux);

        var actes = DecisionDesActes.Decider(hyps, new Referentiel([Eq01])); // CE-01 absent

        Assert.Empty(actes.Elections);
        var refus = Assert.Single(actes.Refus);
        Assert.Equal(Espece.Normatif, refus.Espece);
        Assert.Equal("licenciable-non-licencié", refus.Motif);
        Assert.Equal([1L, 2L], refus.Domaine);
        Assert.Equal(Strate.Contenu, refus.Strate);
    }

    // --- CE-01 élit exactement les hypothèses prévues ---

    [Fact]
    public void CE01_elit_exactement_les_hypotheses_de_la_strate_contenu()
    {
        var hypotheses = HypothesesOracle();

        var actes = DecisionDesActes.Decider(hypotheses, ReferentielReel());

        Assert.Empty(actes.Refus);
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
    }

    // --- déterminisme ---

    [Fact]
    public void Deux_decisions_sur_les_memes_entrees_produisent_le_meme_resultat()
    {
        var hypotheses = HypothesesOracle();
        var referentiel = ReferentielReel();

        var premiere = DecisionDesActes.Decider(hypotheses, referentiel);
        var seconde = DecisionDesActes.Decider(hypotheses, referentiel);

        Assert.Equal(Cles(premiere), Cles(seconde));
    }

    // --- stabilité / reproductibilité entre deux exécutions indépendantes depuis l'oracle et le registre réel ---

    [Fact]
    public void Deux_executions_independantes_depuis_loracle_et_le_registre_produisent_le_meme_resultat()
    {
        var premiere = DecisionDesActes.Decider(HypothesesOracle(), ReferentielReel());
        var seconde = DecisionDesActes.Decider(HypothesesOracle(), ReferentielReel());

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

        var actes = DecisionDesActes.Decider(hyps, new Referentiel([Eq01])); // sans CE-01

        Assert.Empty(actes.Elections);
        Assert.Single(actes.Refus);
    }

    // --- indépendance de l'ordre d'entrée ---

    [Fact]
    public void Lordre_des_hypotheses_ne_change_pas_les_actes_produits()
    {
        var hypotheses = HypothesesOracle();
        var referentiel = ReferentielReel();

        var direct = DecisionDesActes.Decider(hypotheses, referentiel);
        var inverse = DecisionDesActes.Decider(hypotheses.Reverse().ToList(), referentiel);

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
            referentiel);

        Assert.Equal(112, actes.Elections.Count);
        Assert.All(actes.Elections, e =>
        {
            foreach (var acteId in e.Domaine)
            {
                Assert.Equal(e.ContenuPropositionnel, actesParId[acteId].Empreinte);
            }
        });
    }

    // --- adaptateur mémoire identique à SQLite ---

    [Fact]
    public void Ladaptateur_memoire_produit_les_memes_actes_que_le_modele_direct()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var source = new SourceObservationsEnMemoire(modele, []);

        var directs = DecisionDesActes.Decider(
            ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel)),
            referentiel);
        var viaAdaptateur = DecisionDesActes.Decider(
            ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(source.ProjeterModele(), referentiel)),
            referentiel);

        Assert.Equal(Cles(directs), Cles(viaAdaptateur));
    }
}
