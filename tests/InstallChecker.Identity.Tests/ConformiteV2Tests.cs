using InstallChecker;
using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Frontiere;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Identity.Tests;

/// <summary>
/// V2-6 — la conformité assemblée à la frontière du porteur (011 § 8 exercé selon 018 § 8) :
/// W₀ à l'acte près (point 1), déterminisme et registre amputé (point 2), les sept erreurs
/// provoquées sur adaptateurs réels (point 4), les deux sous-classes de l'écart publié
/// (017 § 9), et la garde de coïncidence entre la déclaration consignée
/// (docs/conformite/declaration-conformite-v2.md) et la déclaration du moteur.
/// Le point 3 (audit) est exercé par RestitutionDAuditTests (012 § 8) et PorteurTests.
/// </summary>
public class ConformiteV2Tests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "identity-conformite-tests-" + Guid.NewGuid())).FullName;

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }

    private string NouveauCheminDeBase() => Path.Combine(_root, Guid.NewGuid() + ".db");

    private static string RacineDuDepot()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        return repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable");
    }

    private static LecteurDObservationsSqlite OmegaOracle() =>
        new(Path.Combine(RacineDuDepot(), "tests", "oracle", "corpus1-postA1.db"));

    private static LecteurDeRegistreMarkdown RegistreReel() =>
        new(Path.Combine(RacineDuDepot(), "registre"));

    private static LecteurDeRegistreMarkdown RegistreFixture(string espece, string cas) =>
        new(Path.Combine(AppContext.BaseDirectory, "Fixtures", espece, cas, "registre"));

    private static SourceObservationsEnMemoire OmegaValideEnMemoire() =>
        new(new ModeleObservations([
            new ActeObservation(1, 1, "A", new Dictionary<Attribut, ValeurObservee>()),
            new ActeObservation(2, 1, "A", new Dictionary<Attribut, ValeurObservee>()),
        ]), []);

    private string BaseVersionNonSupportee()
    {
        var chemin = NouveauCheminDeBase();
        using (var store = new ObservationStore(chemin)) store.Commit();
        using var connection = new SqliteConnection($"Data Source={chemin}");
        connection.Open();
        using var commande = connection.CreateCommand();
        commande.CommandText = "PRAGMA user_version = 2;";
        commande.ExecuteNonQuery();
        return chemin;
    }

    private string BaseStructureInattendue()
    {
        var chemin = NouveauCheminDeBase();
        using var connection = new SqliteConnection($"Data Source={chemin}");
        connection.Open();
        using var commande = connection.CreateCommand();
        commande.CommandText = "CREATE TABLE autre_chose (x INTEGER); PRAGMA user_version = 1;";
        commande.ExecuteNonQuery();
        return chemin;
    }

    // --- Point 1 du 011 § 8 : W₀ retrouvé par le porteur, à l'acte près (014 § 8) ---

    [Fact]
    public void Point_1_W0_est_retrouve_par_le_porteur_conformement_au_014_s8()
    {
        var w = Porteur.Deriver(OmegaOracle(), RegistreReel());

        Assert.Equal(1, w.Index.Omega.Version);
        Assert.Equal(497, w.Index.Omega.NombreActes);
        Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], w.Index.Registre);

        Assert.Equal(116, w.Actes.Count);

        var elections = w.Actes.Where(a => a.Type == TypeActe.Election).ToList();
        Assert.Equal(112, elections.Count);
        Assert.Equal(108, elections.Count(e => e.Domaine.Count == 2));
        Assert.Equal(4, elections.Count(e => e.Domaine.Count == 3));
        Assert.All(elections, e =>
        {
            Assert.Equal(Strate.Contenu, e.Strate);
            Assert.Equal(Niveau.Certaine, e.Niveau);
            Assert.Equal("unique-maximale", e.Motif);
            Assert.Equal([new ConventionRef("CE-01", 1)], e.Licences);
            Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], e.Dependances);
            Assert.Empty(e.Dette!);
        });

        var refus = w.Actes.Where(a => a.Type == TypeActe.Refus).ToList();
        Assert.Equal(4, refus.Count);
        Assert.All(refus, r => Assert.Equal(497, r.Domaine.Count));
        Assert.Equal(3, refus.Count(r => r.Motif == "aucune-convention-strate"));
        Assert.Single(refus, r => r.Strate == Strate.Famille && r.Motif == "préalable-absent");
    }

    // --- Point 2 du 011 § 8 : déterminisme par le porteur ; registre amputé d'EQ-01 (EXG-27) ---

    [Fact]
    public void Point_2_deux_derivations_par_le_porteur_sont_identiques_champ_par_champ()
    {
        var premiere = Porteur.Deriver(OmegaOracle(), RegistreReel());
        var seconde = Porteur.Deriver(OmegaOracle(), RegistreReel());

        Assert.Equal(premiere.Index.Omega, seconde.Index.Omega);
        Assert.Equal(premiere.Index.Registre, seconde.Index.Registre);
        Assert.Equal(premiere.Actes.Count, seconde.Actes.Count);
        for (var i = 0; i < premiere.Actes.Count; i++)
        {
            var a = premiere.Actes[i];
            var b = seconde.Actes[i];
            Assert.Equal(a.Type, b.Type);
            Assert.Equal(a.Strate, b.Strate);
            Assert.Equal(a.Domaine, b.Domaine);
            Assert.Equal(a.Contenu, b.Contenu);
            Assert.Equal(a.Niveau, b.Niveau);
            Assert.Equal(a.Motif, b.Motif);
            Assert.Equal(a.Espece, b.Espece);
        }
    }

    [Fact]
    public void Point_2_le_registre_ampute_dEQ01_est_refuse_incoherent_par_le_porteur()
    {
        // EXG-27 : « registre amputé d'EQ-01 → erreur "registre incohérent" » — la dépendance
        // de CE-01 est insatisfaite ; la cohérence précède la couverture (017 § 8).
        Assert.Throws<RegistreIncoherentException>(
            () => Porteur.Deriver(OmegaValideEnMemoire(), RegistreFixture("RegistresCasses", "RegistreIncoherent")));
    }

    // --- Point 4 du 011 § 8 : les sept erreurs provoquées à la frontière, adaptateurs réels ---

    [Fact]
    public void Point_4_les_sept_erreurs_sont_provoquees_a_la_frontiere_du_porteur()
    {
        var registreInexistant = Path.Combine(_root, "registre-inexistant");

        var cas = new (Func<object> Invocation, Type Attendu)[]
        {
            (() => Porteur.Deriver(new LecteurDObservationsSqlite(NouveauCheminDeBase()), RegistreReel()), typeof(OmegaAbsentException)),
            (() => Porteur.Deriver(new LecteurDObservationsSqlite(BaseVersionNonSupportee()), RegistreReel()), typeof(OmegaIncompatibleException)),
            (() => Porteur.Deriver(new LecteurDObservationsSqlite(BaseStructureInattendue()), RegistreReel()), typeof(OmegaInvalideException)),
            (() => Porteur.Deriver(OmegaValideEnMemoire(), new LecteurDeRegistreMarkdown(registreInexistant)), typeof(RegistreAbsentException)),
            (() => Porteur.Deriver(OmegaValideEnMemoire(), RegistreFixture("RegistresCasses", "ChampAbsent")), typeof(RegistreMalformeException)),
            (() => Porteur.Deriver(OmegaValideEnMemoire(), RegistreFixture("RegistresCasses", "RegistreIncoherent")), typeof(RegistreIncoherentException)),
            (() => Porteur.Deriver(OmegaValideEnMemoire(), RegistreFixture("RegistresCasses", "RegistreNonCouvert")), typeof(RegistreNonCouvertException)),
        };

        foreach (var (invocation, attendu) in cas)
        {
            var erreur = Record.Exception(() => invocation());
            Assert.NotNull(erreur);
            Assert.IsType(attendu, erreur);
        }
    }

    // --- L'écart publié (017 § 9) : les deux sous-classes, de bout en bout ---

    [Fact]
    public void Sous_classe_1_un_excedent_de_familles_couvertes_produit_un_W_complet_jamais_lerreur()
    {
        // Interprétation excédentaire (EQ-02) : W complet, les deux conventions appliquées.
        var wInterpretation = Porteur.Deriver(
            OmegaValideEnMemoire(), RegistreFixture("RegistresValides", "AvecEq02"));
        Assert.NotEmpty(wInterpretation.Actes);
        Assert.Contains(new ConventionRef("EQ-02", 1), wInterpretation.Index.Registre);

        // Élection excédentaire (CE-02) : W complet, l'élection licenciée par les deux conventions.
        var wElection = Porteur.Deriver(
            OmegaValideEnMemoire(), RegistreFixture("RegistresValides", "AvecCe02"));
        var election = Assert.Single(wElection.Actes, a => a.Type == TypeActe.Election);
        Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("CE-02", 1)], election.Licences);
    }

    [Fact]
    public void Sous_classe_2_un_excedent_hors_couverture_produit_lerreur_et_aucun_W()
    {
        Assert.Throws<RegistreNonCouvertException>(
            () => Porteur.Deriver(OmegaValideEnMemoire(), RegistreFixture("RegistresCasses", "RegistreNonCouvert")));
    }

    // --- La garde de coïncidence : la déclaration consignée = la déclaration du moteur (017 § 3) ---

    [Fact]
    public void La_declaration_consignee_coincide_avec_la_declaration_du_moteur()
    {
        var chemin = Path.Combine(RacineDuDepot(), "docs", "conformite", "declaration-conformite-v2.md");
        Assert.True(File.Exists(chemin), "la déclaration de conformité v2 doit être consignée dans le dépôt (014 É7, 017 § 3)");

        var texte = File.ReadAllText(chemin);
        Assert.Contains("- interprétation", texte);
        Assert.Contains("- élection", texte);
        Assert.Contains("exactement deux familles", texte);

        // Le moteur déclare la même chose — I62 : la déclaration est l'énoncé opposable, sans divergence.
        Assert.Equal(2, DeclarationDeCouverture.FamillesCouvertes.Count);
        Assert.True(DeclarationDeCouverture.Couvre(Famille.Interpretation));
        Assert.True(DeclarationDeCouverture.Couvre(Famille.Election));

        // Et l'écart publié est matérialisé, comme la déclaration l'annonce (017 § 9).
        Assert.True(
            File.Exists(Path.Combine(RacineDuDepot(), "docs", "conformite", "ecart-publie-v1.md")),
            "l'écart publié doit être matérialisé dans le dépôt (011 § 9, I59)");
    }
}
