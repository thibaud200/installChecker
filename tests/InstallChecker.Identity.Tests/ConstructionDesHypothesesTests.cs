using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Tests;

public class ConstructionDesHypothesesTests
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

    private static ActeObservation Acte(long id, string empreinte, long taille = 1) =>
        new(id, taille, empreinte, new Dictionary<Attribut, ValeurObservee>());

    private static string Cle(Hypothese hypothese) =>
        $"{hypothese.Strate}:{string.Join(",", hypothese.Domaine)}:{hypothese.ContenuPropositionnel}:" +
        $"sig={hypothese.Sig.Count}:obs={hypothese.Obs.Count}";

    // --- aucune convention ⇒ aucune hypothèse ---

    [Fact]
    public void Absence_de_convention_ne_produit_aucune_hypothese()
    {
        var modele = new ModeleObservations([Acte(1, "meme-empreinte"), Acte(2, "meme-empreinte")]);
        var signaux = DerivationDesSignaux.Deriver(modele, new Referentiel([]));

        var hypotheses = ConstructionDesHypotheses.Construire(signaux);

        Assert.Empty(hypotheses);
    }

    // --- EQ-01 seule ⇒ hypothèses attendues ---

    [Fact]
    public void EQ01_seule_produit_une_hypothese_par_classe_de_contenu()
    {
        var modele = new ModeleObservations([
            Acte(1, "A"), Acte(2, "A"),                 // une paire
            Acte(3, "B"), Acte(4, "B"), Acte(5, "B"),   // un triplet
            Acte(6, "C"),                                // singleton -> aucune hypothèse
        ]);
        var eq01 = new Convention("EQ-01", 1, Famille.Interpretation, "test", "test", [], "R1", "test", "test", "test", "test", "test", new DateOnly(2026, 7, 5), "test");
        var signaux = DerivationDesSignaux.Deriver(modele, new Referentiel([eq01]));

        var hypotheses = ConstructionDesHypotheses.Construire(signaux);

        Assert.Equal(2, hypotheses.Count);

        var classeA = Assert.Single(hypotheses, h => h.ContenuPropositionnel == "A");
        Assert.Equal([1L, 2L], classeA.Domaine);
        Assert.Equal(Strate.Contenu, classeA.Strate);
        Assert.Single(classeA.Sig);

        var classeB = Assert.Single(hypotheses, h => h.ContenuPropositionnel == "B");
        Assert.Equal([3L, 4L, 5L], classeB.Domaine);
        Assert.Equal(3, classeB.Sig.Count); // C(3,2) paires
    }

    // --- déterminisme ---

    [Fact]
    public void Deux_constructions_sur_les_memes_signaux_produisent_le_meme_resultat()
    {
        var modele = new ModeleObservations([Acte(1, "X"), Acte(2, "X"), Acte(3, "Y")]);
        var eq01 = new Convention("EQ-01", 1, Famille.Interpretation, "t", "t", [], "R1", "t", "t", "t", "t", "t", new DateOnly(2026, 7, 5), "t");
        var signaux = DerivationDesSignaux.Deriver(modele, new Referentiel([eq01]));

        var premiere = ConstructionDesHypotheses.Construire(signaux);
        var seconde = ConstructionDesHypotheses.Construire(signaux);

        Assert.Equal(premiere.Select(Cle), seconde.Select(Cle));
    }

    // --- stabilité entre deux exécutions indépendantes ---

    [Fact]
    public void Deux_executions_independantes_depuis_loracle_et_le_registre_produisent_le_meme_resultat()
    {
        var premiere = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(ModeleOracle(), ReferentielReel()));
        var seconde = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(ModeleOracle(), ReferentielReel()));

        Assert.Equal(premiere.Select(Cle), seconde.Select(Cle));
    }

    // --- provenance complète ---

    [Fact]
    public void Chaque_hypothese_porte_sa_provenance_complete()
    {
        var modele = new ModeleObservations([Acte(10, "Z"), Acte(20, "Z")]);
        var eq01 = new Convention("EQ-01", 1, Famille.Interpretation, "t", "t", [], "R1", "t", "t", "t", "t", "t", new DateOnly(2026, 7, 5), "t");
        var signaux = DerivationDesSignaux.Deriver(modele, new Referentiel([eq01]));

        var hypothese = Assert.Single(ConstructionDesHypotheses.Construire(signaux));

        Assert.Equal([10L, 20L], hypothese.Domaine);
        Assert.Equal(2, hypothese.Obs.Count);
        Assert.Equal([10L, 20L], hypothese.Obs.Select(o => o.ActeId));
        Assert.All(hypothese.Obs, o => Assert.Equal("empreinte", o.Attribut));
        Assert.Single(hypothese.Sig);
        Assert.Equal(new ConventionRef("EQ-01", 1), Assert.Single(hypothese.ConventionsMobilisees));
        Assert.NotEmpty(hypothese.Justification);
    }

    // --- reconstruction depuis Ω (bout en bout, oracle) ---

    [Fact]
    public void Les_hypotheses_se_reconstruisent_depuis_Omega_via_le_registre_reel()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var actesParId = modele.Actes.ToDictionary(a => a.Identifiant);

        var hypotheses = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel));

        Assert.Equal(112, hypotheses.Count);
        Assert.Equal(108, hypotheses.Count(h => h.Domaine.Count == 2));
        Assert.Equal(4, hypotheses.Count(h => h.Domaine.Count == 3));
        Assert.All(hypotheses, h =>
        {
            foreach (var acteId in h.Domaine)
            {
                Assert.Equal(h.ContenuPropositionnel, actesParId[acteId].Empreinte);
            }
        });
    }

    // --- reconstruction depuis les signaux seuls (sans jamais toucher Ω) ---

    [Fact]
    public void Les_hypotheses_se_reconstruisent_depuis_les_seuls_signaux_sans_modele_domega()
    {
        var type = new TypeDeSignal("contenu-identique", new ConventionRef("EQ-01", 1));
        IReadOnlyList<InstanceDeSignal> signaux =
        [
            new(type, "E", Regime.Exact, [new ObservationConsommee(7, "empreinte"), new ObservationConsommee(9, "empreinte")]),
        ];

        var hypothese = Assert.Single(ConstructionDesHypotheses.Construire(signaux));

        Assert.Equal([7L, 9L], hypothese.Domaine);
        Assert.Equal("E", hypothese.ContenuPropositionnel);
        Assert.Equal(Strate.Contenu, hypothese.Strate);
    }

    // --- indépendance de l'ordre ---

    [Fact]
    public void Lordre_des_signaux_ne_change_pas_les_hypotheses_produites()
    {
        var signauxOriginaux = DerivationDesSignaux.Deriver(ModeleOracle(), ReferentielReel());
        var signauxMelanges = signauxOriginaux.Reverse().ToList();

        var hypothesesOriginales = ConstructionDesHypotheses.Construire(signauxOriginaux);
        var hypothesesMelangees = ConstructionDesHypotheses.Construire(signauxMelanges);

        Assert.Equal(hypothesesOriginales.Select(Cle), hypothesesMelangees.Select(Cle));
    }

    // --- adaptateur mémoire toujours valide (I42) ---

    [Fact]
    public void Ladaptation_memoire_produit_les_memes_hypotheses_que_le_modele_direct()
    {
        var modele = new ModeleObservations([Acte(1, "M"), Acte(2, "M")]);
        var eq01 = new Convention("EQ-01", 1, Famille.Interpretation, "t", "t", [], "R1", "t", "t", "t", "t", "t", new DateOnly(2026, 7, 5), "t");
        var referentiel = new Referentiel([eq01]);
        var source = new SourceObservationsEnMemoire(modele, []);

        var directes = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel));
        var viaAdaptateur = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(source.ProjeterModele(), referentiel));

        Assert.Equal(directes.Select(Cle), viaAdaptateur.Select(Cle));
    }

    // --- aucune hypothèse hors strate contenu ---

    [Fact]
    public void Aucune_hypothese_nest_produite_hors_de_la_strate_contenu()
    {
        var hypotheses = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(ModeleOracle(), ReferentielReel()));

        Assert.NotEmpty(hypotheses);
        Assert.All(hypotheses, h => Assert.Equal(Strate.Contenu, h.Strate));
    }
}
