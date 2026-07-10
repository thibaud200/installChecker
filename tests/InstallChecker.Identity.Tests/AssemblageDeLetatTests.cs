using System.Security.Cryptography;
using System.Text;
using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Tests;

public class AssemblageDeLetatTests
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

    private static W AssemblerDepuisOracle(ModeleObservations modele, Referentiel referentiel)
    {
        var hypotheses = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel));
        var identifiants = modele.Actes.Select(a => a.Identifiant).ToList();
        var actes = DecisionDesActes.Decider(hypotheses, referentiel, identifiants);
        var index = new IndexEtat(new SourceObservationsEnMemoire(modele, []).ProjeterIdentite(), referentiel.Index);
        return AssemblageDeLetat.Assembler(actes, index);
    }

    private static W AssemblerW0() => AssemblerDepuisOracle(ModeleOracle(), ReferentielReel());

    private static string JoinRefs(IReadOnlyList<ConventionRef>? refs) => refs is null ? "-" : string.Join(";", refs);

    private static string Cle(ActeW a) =>
        $"{a.Type}:{a.Strate}:[{string.Join(",", a.Domaine)}]:{a.Contenu}:{a.Niveau}:{a.Motif}:{a.Espece}:" +
        $"lic=[{JoinRefs(a.Licences)}]:dep=[{JoinRefs(a.Dependances)}]:dette=[{JoinRefs(a.Dette)}]";

    private static string Cle(W w) =>
        $"omega={w.Index.Omega.Version}:{w.Index.Omega.NombreActes}:{w.Index.Omega.EmpreinteEtat}|" +
        $"registre=[{string.Join(";", w.Index.Registre)}]|" +
        string.Join("||", w.Actes.Select(Cle));

    // --- conformité exacte à W₀ (116 actes, 014 § 8) ---

    [Fact]
    public void Le_pipeline_complet_produit_exactement_W0()
    {
        var w = AssemblerW0();

        Assert.Equal(1, w.Index.Omega.Version);
        Assert.Equal(497, w.Index.Omega.NombreActes);
        Assert.Matches("^[0-9a-f]{64}$", w.Index.Omega.EmpreinteEtat);
        Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], w.Index.Registre);

        Assert.Equal(116, w.Actes.Count);

        var elections = w.Actes.Where(a => a.Type == TypeActe.Election).ToList();
        var refus = w.Actes.Where(a => a.Type == TypeActe.Refus).ToList();
        Assert.Equal(112, elections.Count);
        Assert.Equal(4, refus.Count);
        Assert.Equal(108, elections.Count(a => a.Domaine.Count == 2));
        Assert.Equal(4, elections.Count(a => a.Domaine.Count == 3));
        Assert.All(elections, a =>
        {
            Assert.Equal(Strate.Contenu, a.Strate);
            Assert.Equal(Niveau.Certaine, a.Niveau);
            Assert.Equal("unique-maximale", a.Motif);
            Assert.Equal([new ConventionRef("CE-01", 1)], a.Licences);
            Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], a.Dependances);
            Assert.Empty(a.Dette!);
        });

        Assert.Equal([Strate.Variante, Strate.Version, Strate.Identite, Strate.Famille], refus.Select(r => r.Strate));
        Assert.All(refus.Take(3), r => Assert.Equal("aucune-convention-strate", r.Motif));
        Assert.Equal("préalable-absent", refus[3].Motif);
        Assert.All(refus, r =>
        {
            Assert.Equal(Espece.Normatif, r.Espece);
            Assert.Equal(497, r.Domaine.Count);
        });
    }

    // --- conformité bit-à-bit entre deux calculs indépendants ---

    [Fact]
    public void Conformite_bit_a_bit_entre_deux_calculs_independants_de_W0()
    {
        var premier = AssemblerDepuisOracle(ModeleOracle(), ReferentielReel());
        var second = AssemblerDepuisOracle(ModeleOracle(), ReferentielReel());

        Assert.Equal(Cle(premier), Cle(second));
    }

    // --- déterminisme de l'assemblage sur les mêmes actes ---

    [Fact]
    public void Deux_assemblages_sur_le_meme_ensemble_dactes_produisent_le_meme_W()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel));
        var identifiants = modele.Actes.Select(a => a.Identifiant).ToList();
        var actes = DecisionDesActes.Decider(hypotheses, referentiel, identifiants);
        var index = new IndexEtat(new SourceObservationsEnMemoire(modele, []).ProjeterIdentite(), referentiel.Index);

        var premier = AssemblageDeLetat.Assembler(actes, index);
        var second = AssemblageDeLetat.Assembler(actes, index);

        Assert.Equal(Cle(premier), Cle(second));
    }

    // --- indépendance de l'ordre des actes en entrée ---

    [Fact]
    public void Lordre_des_actes_en_entree_ne_change_pas_le_W_assemble()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var hypotheses = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel));
        var identifiants = modele.Actes.Select(a => a.Identifiant).ToList();
        var actes = DecisionDesActes.Decider(hypotheses, referentiel, identifiants);
        var actesMelanges = new EnsembleDesActes(actes.Elections.Reverse().ToList(), actes.Refus.Reverse().ToList());
        var index = new IndexEtat(new SourceObservationsEnMemoire(modele, []).ProjeterIdentite(), referentiel.Index);

        var direct = AssemblageDeLetat.Assembler(actes, index);
        var melange = AssemblageDeLetat.Assembler(actesMelanges, index);

        Assert.Equal(Cle(direct), Cle(melange));
    }

    // --- ordre canonique des actes (014 § 7.3) ---

    [Fact]
    public void Lordre_canonique_est_strate_puis_type_puis_plus_petit_identifiant()
    {
        var w = AssemblerW0();

        var elections = w.Actes.Take(112).ToList();
        var refus = w.Actes.Skip(112).ToList();

        Assert.All(elections, a => Assert.Equal(TypeActe.Election, a.Type));
        Assert.Equal(elections.Select(a => a.Domaine[0]), elections.Select(a => a.Domaine[0]).OrderBy(id => id));
        Assert.Equal([Strate.Variante, Strate.Version, Strate.Identite, Strate.Famille], refus.Select(r => r.Strate));
    }

    // --- absence de champs réservés à l'autre type (014 § 7.3 : « — ») ---

    [Fact]
    public void Les_champs_reserves_a_lautre_type_sont_absents()
    {
        var w = AssemblerW0();

        Assert.All(w.Actes.Where(a => a.Type == TypeActe.Election), a =>
        {
            Assert.NotNull(a.Contenu);
            Assert.NotNull(a.Niveau);
            Assert.Null(a.Espece);
            Assert.NotNull(a.Licences);
            Assert.NotNull(a.Dependances);
            Assert.NotNull(a.Dette);
        });

        Assert.All(w.Actes.Where(a => a.Type == TypeActe.Refus), a =>
        {
            Assert.Null(a.Contenu);
            Assert.Null(a.Niveau);
            Assert.NotNull(a.Espece);
            Assert.Null(a.Licences);
            Assert.Null(a.Dependances);
            Assert.Null(a.Dette);
        });
    }

    // --- empreinte d'état conforme au 014 § 7.2 raffiné (025 § 3) : la fonction déclarée du
    //     support sur l'encodage à préfixe de longueur des couples (identifiant, empreinte) ---

    [Fact]
    public void Lempreinte_detat_est_la_fonction_declaree_du_support_sur_lencodage_des_couples()
    {
        var modele = ModeleOracle();

        var encodage = string.Concat(modele.Actes
            .OrderBy(a => a.Identifiant)
            .SelectMany(a => new[] { a.Identifiant.ToString(), a.Empreinte })
            .Select(v => $"{v.Length}:{v},"));
        var attendue = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(encodage)));

        Assert.Equal(attendue, new SourceObservationsEnMemoire(modele, []).ProjeterIdentite().EmpreinteEtat);
    }

    // --- V3-7 : la discrimination (025 § 1, P1) — les identifiants entrent dans l'identité ---

    [Fact]
    public void Deux_etats_de_memes_contenus_et_didentifiants_differents_ont_des_identites_distinctes()
    {
        // La classe des renumérotations, jadis confondue sur un même index (report 5) :
        // mêmes contenus (A, B), identifiants (1, 2) contre (5, 9) — deux identités désormais.
        var premier = new ModeleObservations([
            new ActeObservation(1, 1, "A", new Dictionary<Attribut, ValeurObservee>()),
            new ActeObservation(2, 1, "B", new Dictionary<Attribut, ValeurObservee>()),
        ]);
        var second = new ModeleObservations([
            new ActeObservation(5, 1, "A", new Dictionary<Attribut, ValeurObservee>()),
            new ActeObservation(9, 1, "B", new Dictionary<Attribut, ValeurObservee>()),
        ]);

        var identitePremier = new SourceObservationsEnMemoire(premier, []).ProjeterIdentite();
        var identiteSecond = new SourceObservationsEnMemoire(second, []).ProjeterIdentite();

        Assert.NotEqual(identitePremier.EmpreinteEtat, identiteSecond.EmpreinteEtat);
        Assert.Equal(identitePremier.NombreActes, identiteSecond.NombreActes);

        // Et le déterminisme demeure : le même état produit la même identité.
        Assert.Equal(
            identitePremier.EmpreinteEtat,
            new SourceObservationsEnMemoire(premier, []).ProjeterIdentite().EmpreinteEtat);
    }

    // --- reconstruction complète depuis Ω et ℛ via le pipeline ---

    [Fact]
    public void W_se_reconstruit_integralement_depuis_Omega_et_le_registre_reel()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var actesParId = modele.Actes.ToDictionary(a => a.Identifiant);

        var w = AssemblerDepuisOracle(modele, referentiel);

        Assert.All(w.Actes.Where(a => a.Type == TypeActe.Election), a =>
        {
            foreach (var acteId in a.Domaine)
            {
                Assert.Equal(a.Contenu, actesParId[acteId].Empreinte);
            }
        });
    }

    // --- équivalence adaptateur mémoire / SQLite ---

    [Fact]
    public void Ladaptateur_memoire_produit_le_meme_W_que_le_modele_direct()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var source = new SourceObservationsEnMemoire(modele, []);

        var direct = AssemblerDepuisOracle(modele, referentiel);
        var viaAdaptateur = AssemblerDepuisOracle(source.ProjeterModele(), referentiel);

        Assert.Equal(Cle(direct), Cle(viaAdaptateur));
    }

    // --- localité (EXG-21, P3) : un acte sans rapport ajouté à Ω ne change aucune élection existante ---

    [Fact]
    public void Lajout_dun_acte_sans_rapport_ne_change_aucune_election_existante()
    {
        var modele = ModeleOracle();
        var referentiel = ReferentielReel();
        var w0 = AssemblerDepuisOracle(modele, referentiel);

        var acteSansRapport = new ActeObservation(
            modele.Actes.Max(a => a.Identifiant) + 1, 1, "empreinte-inedite-sans-aucun-rapport",
            new Dictionary<Attribut, ValeurObservee>());
        var modeleEtendu = new ModeleObservations([.. modele.Actes, acteSansRapport]);
        var w1 = AssemblerDepuisOracle(modeleEtendu, referentiel);

        var electionsOriginales = w0.Actes.Where(a => a.Type == TypeActe.Election).Select(Cle).ToHashSet();
        var electionsEtendues = w1.Actes.Where(a => a.Type == TypeActe.Election).Select(Cle).ToHashSet();

        Assert.Equal(112, electionsOriginales.Count);
        Assert.Equal(112, electionsEtendues.Count); // l'acte ajouté est un singleton : aucune élection nouvelle
        Assert.Equal(electionsOriginales, electionsEtendues); // toutes conservées à l'identique, champ pour champ

        // Seuls les refus structurels grandissent : leur domaine EST « tous les actes d'Ω » (009 § 6) —
        // il croît avec Ω par construction, ce qui borne exactement ce que la localité promet ici.
        Assert.All(w1.Actes.Where(a => a.Type == TypeActe.Refus), r => Assert.Equal(498, r.Domaine.Count));
    }

    // --- B (audit final, A5) : Ω vide traverse tout le pipeline et produit un W entier, sans acte — jamais une exception (011 §§ 4-5) ---

    [Fact]
    public void Omega_vide_produit_un_W_entier_sans_acte()
    {
        var modele = new ModeleObservations([]);
        var referentiel = ReferentielReel();

        var w = AssemblerDepuisOracle(modele, referentiel);

        Assert.Equal(0, w.Index.Omega.NombreActes);
        Assert.Matches("^[0-9a-f]{64}$", w.Index.Omega.EmpreinteEtat);
        Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], w.Index.Registre);
        Assert.Empty(w.Actes);
    }

    // --- calcul correct de τ (006 § 7, 014 § 7.5) ---

    [Fact]
    public void Tau_classe_correctement_conserve_abandonne_et_nouveau()
    {
        var indexAvant = new IndexEtat(new IndexOmega(1, 3, "empreinte-avant"), [new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)]);
        var indexApres = new IndexEtat(new IndexOmega(1, 5, "empreinte-apres"), [new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)]);

        var electionConservee = new ActeW(TypeActe.Election, Strate.Contenu, [1, 2], "A", Niveau.Certaine, "unique-maximale",
            null, [new ConventionRef("CE-01", 1)], [new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], []);
        var refusVariante = new ActeW(TypeActe.Refus, Strate.Variante, [1, 2, 3], null, null, "aucune-convention-strate",
            Espece.Normatif, null, null, null);
        var electionNouvelle = new ActeW(TypeActe.Election, Strate.Contenu, [4, 5], "B", Niveau.Certaine, "unique-maximale",
            null, [new ConventionRef("CE-01", 1)], [new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], []);

        var avant = new W(indexAvant, [electionConservee, refusVariante]);
        var apres = new W(indexApres, [electionConservee, electionNouvelle]);

        var tau = AssemblageDeLetat.CalculerTransition(avant, apres, [1, 2, 3], [1, 2, 3, 4, 5]);

        Assert.Equal([new ReferenceActe(Strate.Contenu, [1, 2])], tau.Correspondance.Conserves);
        Assert.Equal([new ReferenceActe(Strate.Variante, [1, 2, 3])], tau.Correspondance.Abandonnes);
        Assert.Equal([new ReferenceActe(Strate.Contenu, [4, 5])], tau.Correspondance.Nouveaux);
        Assert.Equal(indexAvant, tau.IndexAvant);
        Assert.Equal(indexApres, tau.IndexApres);

        // La cause est dérivée des entrées, jamais fournie (026 § 3) : les identités d'Ω diffèrent →
        // volet Ω (le delta des énumérations) ; les listes ℛ sont égales → aucun volet ℛ.
        Assert.NotNull(tau.Cause.Omega);
        Assert.Equal([4L, 5L], tau.Cause.Omega.Ajoutes);
        Assert.Empty(tau.Cause.Omega.Retires);
        Assert.Null(tau.Cause.Registre);

        // La continuité triviale de l'élection conservée (026 § 4, 006 E5) ; l'élection nouvelle
        // n'a aucun prédécesseur (contenu « B » absent de W).
        Assert.Equal(
            [(new ReferenceActe(Strate.Contenu, [1, 2]), new ReferenceActe(Strate.Contenu, [1, 2]))],
            tau.Correspondance.Continuites);
    }

    // --- V3-6 : la totalité de la référence (024 § 3, report 8) ---

    [Fact]
    public void Tau_reste_total_quand_deux_actes_dune_meme_strate_partagent_leur_plus_petit_identifiant()
    {
        // Le cas constructible du report 8 : une élection {1, 2} et un refus {1, 3} à la même
        // strate — même plus petit identifiant, deux actes. La référence-identité (strate, domaine)
        // les distingue ; la paire abrégée serait entrée en collision (024 § 2).
        var index = new IndexEtat(new IndexOmega(1, 3, "empreinte"), [new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)]);
        var election = new ActeW(TypeActe.Election, Strate.Contenu, [1, 2], "A", Niveau.Certaine, "unique-maximale",
            null, [new ConventionRef("CE-01", 1)], [new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], []);
        var refusRecoupant = new ActeW(TypeActe.Refus, Strate.Contenu, [1, 3], null, null, "sous-détermination",
            Espece.Structurel, null, null, null);

        var w = new W(index, [election, refusRecoupant]);

        var tau = AssemblageDeLetat.CalculerTransition(w, w, [1, 2, 3], [1, 2, 3]);

        Assert.Equal(2, tau.Correspondance.Conserves.Count);
        Assert.Contains(new ReferenceActe(Strate.Contenu, [1, 2]), tau.Correspondance.Conserves);
        Assert.Contains(new ReferenceActe(Strate.Contenu, [1, 3]), tau.Correspondance.Conserves);
        Assert.Empty(tau.Correspondance.Abandonnes);
        Assert.Empty(tau.Correspondance.Nouveaux);

        // Deux index égaux : la cause est vide — une comparaison, jamais une révision (026 § 3, 006 Déf. 6).
        Assert.Null(tau.Cause.Omega);
        Assert.Null(tau.Cause.Registre);
    }
}
