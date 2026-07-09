using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Observations;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Tests;

public class DerivationDesSignauxTests
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

    private static Convention ConventionEq01(int version = 1) => new(
        Identifiant: "EQ-01",
        Version: version,
        Famille: Famille.Interpretation,
        DomaineApplication: "test",
        Transformation: "test",
        Dependances: [],
        RegimesAdmis: "R1",
        Portee: "test",
        Justification: "test",
        JustificationEmpirique: "test",
        Limites: "test",
        ConditionsDeRevision: "test",
        Date: new DateOnly(2026, 7, 5),
        Autorite: "test");

    private static ActeObservation Acte(long id, string empreinte, long taille = 1) =>
        new(id, taille, empreinte, new Dictionary<Attribut, ValeurObservee>());

    private static string Cle(InstanceDeSignal instance) =>
        $"{instance.Provenance[0].ActeId}-{instance.Provenance[1].ActeId}:{instance.Sortie}:{instance.Regime}:" +
        $"{instance.Type.Identifiant}:{instance.Type.Convention.Identifiant}v{instance.Type.Convention.Version}";

    // --- I13 : aucune instance sans convention fondatrice ---

    [Fact]
    public void Absence_de_convention_ne_produit_aucun_signal_meme_avec_contenu_partage()
    {
        var modele = new ModeleObservations([Acte(1, "meme-empreinte"), Acte(2, "meme-empreinte")]);
        var referentiel = new Referentiel([]);

        var signaux = DerivationDesSignaux.Deriver(modele, referentiel);

        Assert.Empty(signaux);
    }

    // --- EQ-01 seule ⇒ signaux attendus ---

    [Fact]
    public void EQ01_seule_produit_un_signal_par_paire_de_meme_contenu()
    {
        var modele = new ModeleObservations([
            Acte(1, "A"), Acte(2, "A"),         // une paire
            Acte(3, "B"), Acte(4, "B"), Acte(5, "B"), // un triplet -> 3 paires
            Acte(6, "C"),                        // singleton -> aucune paire
        ]);
        var referentiel = new Referentiel([ConventionEq01()]);

        var signaux = DerivationDesSignaux.Deriver(modele, referentiel);

        Assert.Equal(4, signaux.Count); // 1 (classe A) + 3 (classe B)
        Assert.All(signaux, s =>
        {
            Assert.Equal(Regime.Exact, s.Regime);
            Assert.Equal("contenu-identique", s.Type.Identifiant);
            Assert.Equal(new ConventionRef("EQ-01", 1), s.Type.Convention);
        });
        Assert.Equal(3, signaux.Count(s => s.Sortie == "B"));
        Assert.Equal(1, signaux.Count(s => s.Sortie == "A"));
    }

    [Fact]
    public void CE01_en_vigueur_naffecte_pas_la_derivation_de_C3()
    {
        // ℛ0 réel contient CE-01 (élection) en plus d'EQ-01 : C3 doit produire exactement
        // ce qu'EQ-01 seule licencierait — CE-01 n'est d'aucune famille que C3 consulte (012 § 1.2).
        var referentiel = ReferentielReel();
        var modele = ModeleOracle();

        var signaux = DerivationDesSignaux.Deriver(modele, referentiel);

        var classesMulti = modele.Actes.GroupBy(a => a.Empreinte).Where(g => g.Count() >= 2).ToList();
        Assert.Equal(108, classesMulti.Count(g => g.Count() == 2));
        Assert.Equal(4, classesMulti.Count(g => g.Count() == 3));

        // 108 paires (1 chacune) + 4 triplets (3 paires chacun, C(3,2)=3) = 120.
        Assert.Equal(120, signaux.Count);
        Assert.All(signaux, s => Assert.Equal(new ConventionRef("EQ-01", 1), s.Type.Convention));
    }

    // --- Déterminisme et stabilité (I6) ---

    [Fact]
    public void Deux_derivations_en_memoire_produisent_le_meme_resultat()
    {
        var modele = new ModeleObservations([Acte(1, "X"), Acte(2, "X"), Acte(3, "Y")]);
        var referentiel = new Referentiel([ConventionEq01()]);

        var premiere = DerivationDesSignaux.Deriver(modele, referentiel);
        var seconde = DerivationDesSignaux.Deriver(modele, referentiel);

        Assert.Equal(premiere.Select(Cle), seconde.Select(Cle));
    }

    [Fact]
    public void Deux_executions_independantes_depuis_loracle_et_le_registre_produisent_le_meme_resultat()
    {
        var premiere = DerivationDesSignaux.Deriver(ModeleOracle(), ReferentielReel());
        var seconde = DerivationDesSignaux.Deriver(ModeleOracle(), ReferentielReel());

        Assert.Equal(premiere.Select(Cle), seconde.Select(Cle));
    }

    // --- Provenance complète et reconstructibilité depuis Ω (I5, I7, I30) ---

    [Fact]
    public void Chaque_instance_porte_sa_provenance_complete()
    {
        var modele = new ModeleObservations([Acte(10, "Z"), Acte(20, "Z")]);
        var referentiel = new Referentiel([ConventionEq01()]);

        var signal = Assert.Single(DerivationDesSignaux.Deriver(modele, referentiel));

        Assert.Equal(2, signal.Provenance.Count);
        Assert.Equal([10, 20], signal.Provenance.Select(o => o.ActeId));
        Assert.All(signal.Provenance, o => Assert.Equal("empreinte", o.Attribut));
    }

    [Fact]
    public void Chaque_signal_est_reconstructible_depuis_les_actes_dOmega()
    {
        var referentiel = ReferentielReel();
        var modele = ModeleOracle();
        var actesParId = modele.Actes.ToDictionary(a => a.Identifiant);

        var signaux = DerivationDesSignaux.Deriver(modele, referentiel);

        Assert.NotEmpty(signaux);
        Assert.All(signaux, signal =>
        {
            foreach (var observation in signal.Provenance)
            {
                var acte = actesParId[observation.ActeId]; // lève si l'acte n'existe pas dans Ω
                Assert.Equal(signal.Sortie, acte.Empreinte); // la sortie coïncide avec l'attribut cité en provenance
            }
        });
    }

    // --- Indépendance de l'ordre des observations ---

    [Fact]
    public void Lordre_des_actes_dans_le_modele_ne_change_pas_les_signaux_produits()
    {
        var actesOriginaux = ModeleOracle().Actes;
        var actesMelanges = actesOriginaux.Reverse().ToList(); // ordre physiquement différent, même contenu
        var referentiel = ReferentielReel();

        var signauxOriginaux = DerivationDesSignaux.Deriver(new ModeleObservations(actesOriginaux), referentiel);
        var signauxMelanges = DerivationDesSignaux.Deriver(new ModeleObservations(actesMelanges), referentiel);

        Assert.Equal(signauxOriginaux.Select(Cle), signauxMelanges.Select(Cle));
    }

    // --- Indépendance de l'ordre des conventions (I31 étendu à ℛ, EXG-27) ---

    [Fact]
    public void Lordre_des_conventions_dans_le_referentiel_ne_change_pas_les_signaux_produits()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var referentielInverse = new Referentiel(referentiel.ConventionsEnVigueur.Reverse().ToList());

        var direct = DerivationDesSignaux.Deriver(modele, referentiel);
        var inverse = DerivationDesSignaux.Deriver(modele, referentielInverse);

        Assert.Equal(direct.Select(Cle), inverse.Select(Cle));
    }

    // --- Substituabilité de l'adaptateur en mémoire (I42) ---

    [Fact]
    public void Ladaptation_memoire_produit_les_memes_signaux_que_le_modele_direct()
    {
        var modele = new ModeleObservations([Acte(1, "M"), Acte(2, "M")]);
        var referentiel = new Referentiel([ConventionEq01()]);
        var source = new SourceObservationsEnMemoire(modele, []);

        var directs = DerivationDesSignaux.Deriver(modele, referentiel);
        var viaAdaptateur = DerivationDesSignaux.Deriver(source.ProjeterModele(), referentiel);

        Assert.Equal(directs.Select(Cle), viaAdaptateur.Select(Cle));
    }

    // --- Intégration C2 → C3 : un registre incohérent ne laisse jamais C3 s'exécuter ---

    [Fact]
    public void Registre_incoherent_est_refuse_par_C2_avant_que_C3_ne_sexecute()
    {
        var cheminFixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "RegistresCasses", "RegistreIncoherent", "registre");
        var lecteur = new LecteurDeRegistreMarkdown(cheminFixture);
        var c3Execute = false;

        Assert.Throws<RegistreIncoherentException>(() =>
        {
            var referentiel = lecteur.Projeter(); // lève ici : C3 n'est jamais atteint
            DerivationDesSignaux.Deriver(new ModeleObservations([]), referentiel);
            c3Execute = true;
        });

        Assert.False(c3Execute);
    }

    // --- V2-3 : l'application par famille (017 § 2, report 1) ---

    private static Convention ConventionInterpretation(string identifiant, int version = 1) =>
        ConventionEq01(version) with { Identifiant = identifiant };

    [Fact]
    public void EQ02_seule_fonde_ses_propres_signaux()
    {
        var modele = new ModeleObservations([Acte(1, "A"), Acte(2, "A")]);
        var referentiel = new Referentiel([ConventionInterpretation("EQ-02")]);

        var signal = Assert.Single(DerivationDesSignaux.Deriver(modele, referentiel));

        Assert.Equal(new ConventionRef("EQ-02", 1), signal.Type.Convention);
        Assert.Equal("contenu-identique", signal.Type.Identifiant);
        Assert.Equal(Regime.Exact, signal.Regime);
    }

    [Fact]
    public void Deux_conventions_dinterpretation_coexistent_chacune_fondant_ses_instances()
    {
        // I13 : chaque instance cite sa convention fondatrice — deux conventions en vigueur
        // de la même famille fondent chacune leurs propres instances sur les mêmes paires.
        var modele = new ModeleObservations([Acte(1, "A"), Acte(2, "A")]);
        var referentiel = new Referentiel([ConventionEq01(), ConventionInterpretation("EQ-02")]);

        var signaux = DerivationDesSignaux.Deriver(modele, referentiel);

        Assert.Equal(2, signaux.Count);
        Assert.Single(signaux, s => s.Type.Convention == new ConventionRef("EQ-01", 1));
        Assert.Single(signaux, s => s.Type.Convention == new ConventionRef("EQ-02", 1));
        Assert.All(signaux, s => Assert.Equal([1L, 2L], s.Provenance.Select(o => o.ActeId)));
    }

    [Fact]
    public void La_selection_depend_uniquement_de_la_famille_jamais_de_lidentifiant()
    {
        var modele = new ModeleObservations([Acte(1, "A"), Acte(2, "A")]);

        // Une convention nommée EQ-01 mais d'une autre famille ne fonde rien en C3…
        var horsFamille = new Referentiel([ConventionEq01() with { Famille = Famille.Election }]);
        Assert.Empty(DerivationDesSignaux.Deriver(modele, horsFamille));

        // … et une convention d'identifiant quelconque de la famille interprétation fonde ses signaux.
        var identifiantQuelconque = new Referentiel([ConventionInterpretation("XX-99")]);
        var signal = Assert.Single(DerivationDesSignaux.Deriver(modele, identifiantQuelconque));
        Assert.Equal(new ConventionRef("XX-99", 1), signal.Type.Convention);
    }

    [Fact]
    public void Apprentissage_sans_changement_de_moteur_un_registre_enrichi_dEQ02_est_applique_de_bout_en_bout()
    {
        // La promesse du 016 § 5.1 et du 017 § 2 : une convention supplémentaire d'une famille
        // couverte est une donnée — le registre traverse C2 (couvert), et C3 l'applique,
        // sans qu'une seule ligne du moteur ait changé.
        var cheminFixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "RegistresValides", "AvecEq02", "registre");
        var referentiel = new LecteurDeRegistreMarkdown(cheminFixture).Projeter();
        var modele = new ModeleObservations([Acte(1, "A"), Acte(2, "A")]);

        var signaux = DerivationDesSignaux.Deriver(modele, referentiel);

        Assert.Equal(2, signaux.Count);
        Assert.Single(signaux, s => s.Type.Convention == new ConventionRef("EQ-01", 1));
        Assert.Single(signaux, s => s.Type.Convention == new ConventionRef("EQ-02", 1));
    }
}
