using System.Text.Json;
using InstallChecker.Identity.Access.Registre;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

/// <summary>
/// V2-7 (É9, 018 § 6) — la commande <c>identity</c>, consommateur du moteur : bout-en-bout sur
/// l'artefact et le registre réels, erreurs restituées telles quelles, batterie des sept erreurs
/// par la commande, audit restitué unité par unité. Ces tests vivent dans la suite de la CLI —
/// c'est le composant qu'ils exercent (jalon V3-1, report 10 : la suite Identity redevient pure) ;
/// les fixtures de registres, versionnées avec la suite du moteur, sont lues par chemin de dépôt.
/// </summary>
public class IdentityCommandTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "identity-cli-tests-" + Guid.NewGuid())).FullName;

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

    private static string CheminOracle() => Path.Combine(RacineDuDepot(), "tests", "oracle", "corpus1-postA1.db");

    private static string CheminRegistreReel() => Path.Combine(RacineDuDepot(), "registre");

    private static string CheminFixture(string espece, string cas) =>
        Path.Combine(RacineDuDepot(), "tests", "InstallChecker.Identity.Tests", "Fixtures", espece, cas, "registre");

    private string BaseVersionNonSupportee()
    {
        var chemin = NouveauCheminDeBase();
        using (var store = new ObservationStore(chemin,
            new ScanDeclaration("vol-test", null, @"C:\", "2026-01-01T00:00:00Z", null))) store.Commit();
        using var connection = new SqliteConnection($"Data Source={chemin}");
        connection.Open();
        using var commande = connection.CreateCommand();
        commande.CommandText = "PRAGMA user_version = 3;";
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

    // --- É9 : bout-en-bout sur l'artefact — l'émission de W ---

    [Fact]
    public void Derive_emet_W0_de_bout_en_bout_sur_lartefact_et_le_registre_reels()
    {
        var sortie = new StringWriter();
        var erreurs = new StringWriter();

        var code = IdentityCommand.Deriver(CheminOracle(), CheminRegistreReel(), sortie, erreurs);

        Assert.Equal(0, code);
        Assert.Empty(erreurs.ToString());

        using var json = JsonDocument.Parse(sortie.ToString());
        var racine = json.RootElement;

        Assert.Equal(497, racine.GetProperty("index").GetProperty("omega").GetProperty("nombreActes").GetInt32());
        Assert.Matches("^[0-9a-f]{64}$", racine.GetProperty("index").GetProperty("omega").GetProperty("empreinteEtat").GetString());
        Assert.Equal(2, racine.GetProperty("index").GetProperty("registre").GetArrayLength());

        var actes = racine.GetProperty("actes");
        Assert.Equal(116, actes.GetArrayLength());
        Assert.Equal(112, actes.EnumerateArray().Count(a => a.GetProperty("type").GetString() == "élection"));
        Assert.Equal(4, actes.EnumerateArray().Count(a => a.GetProperty("type").GetString() == "refus"));
    }

    [Fact]
    public void Deux_emissions_du_consommateur_sont_identiques()
    {
        var premiere = new StringWriter();
        var seconde = new StringWriter();

        Assert.Equal(0, IdentityCommand.Deriver(CheminOracle(), CheminRegistreReel(), premiere, new StringWriter()));
        Assert.Equal(0, IdentityCommand.Deriver(CheminOracle(), CheminRegistreReel(), seconde, new StringWriter()));

        Assert.Equal(premiere.ToString(), seconde.ToString());
    }

    [Fact]
    public void Le_consommateur_emet_exactement_le_fichier_W0_attendu_de_loracle_independant()
    {
        // 018 § 6 : « émet W tel que produit, sous la forme canonique du 013 § 4 » — le test d'or
        // par la commande réelle : l'égalité est bit à bit avec le fichier produit hors moteur (É7).
        var sortie = new StringWriter { NewLine = "\n" };

        Assert.Equal(0, IdentityCommand.Deriver(CheminOracle(), CheminRegistreReel(), sortie, new StringWriter()));

        var attendu = File.ReadAllBytes(Path.Combine(RacineDuDepot(), "tests", "oracle", "W0-attendu.json"));
        Assert.Equal(attendu, System.Text.Encoding.UTF8.GetBytes(sortie.ToString()));
    }

    // --- 018 § 6 : toute erreur restituée telle quelle — jamais traduite, renommée ni agrégée ---

    [Fact]
    public void Lerreur_restituee_par_la_commande_est_exactement_le_message_du_moteur()
    {
        var cheminRegistre = CheminFixture("RegistresCasses", "RegistreNonCouvert");
        var directe = Record.Exception(() => new LecteurDeRegistreMarkdown(cheminRegistre).Projeter());
        Assert.NotNull(directe);

        var erreurs = new StringWriter();
        var code = IdentityCommand.Deriver(CheminOracle(), cheminRegistre, new StringWriter(), erreurs);

        Assert.Equal(1, code);
        Assert.Equal(directe.Message, erreurs.ToString().TrimEnd());
    }

    [Fact]
    public void Les_sept_erreurs_du_contrat_sont_restituees_par_la_commande()
    {
        var cas = new (string Base, string Registre)[]
        {
            (NouveauCheminDeBase(), CheminRegistreReel()),                                   // Ω absent
            (BaseVersionNonSupportee(), CheminRegistreReel()),                               // Ω incompatible
            (BaseStructureInattendue(), CheminRegistreReel()),                               // Ω invalide
            (CheminOracle(), Path.Combine(_root, "registre-inexistant")),                    // registre absent
            (CheminOracle(), CheminFixture("RegistresCasses", "ChampAbsent")),               // registre malformé
            (CheminOracle(), CheminFixture("RegistresCasses", "RegistreIncoherent")),        // registre incohérent
            (CheminOracle(), CheminFixture("RegistresCasses", "RegistreNonCouvert")),        // registre non couvert
        };

        foreach (var (cheminBase, cheminRegistre) in cas)
        {
            var erreurs = new StringWriter();
            var code = IdentityCommand.Deriver(cheminBase, cheminRegistre, new StringWriter(), erreurs);

            Assert.Equal(1, code);
            Assert.False(string.IsNullOrWhiteSpace(erreurs.ToString())); // nommée, jamais silencieuse
        }
    }

    // --- É9 : la restitution d'audit, unité par unité ---

    [Fact]
    public void Laudit_est_restitue_par_la_commande_sur_un_acte_designe()
    {
        // Le plus petit identifiant du premier domaine élu : la référence d'acte du 014 § 7.5.
        var sortieW = new StringWriter();
        Assert.Equal(0, IdentityCommand.Deriver(CheminOracle(), CheminRegistreReel(), sortieW, new StringWriter()));
        using var w = JsonDocument.Parse(sortieW.ToString());
        var premierElu = w.RootElement.GetProperty("actes").EnumerateArray()
            .First(a => a.GetProperty("type").GetString() == "élection")
            .GetProperty("domaine")[0].GetInt64();

        var sortie = new StringWriter();
        var code = IdentityCommand.Auditer(
            CheminOracle(), CheminRegistreReel(), "conventions", "contenu", premierElu, sortie, new StringWriter());

        Assert.Equal(0, code);
        Assert.Contains("CE-01", sortie.ToString());
        Assert.Contains("EQ-01", sortie.ToString());
    }

    [Fact]
    public void Une_question_ou_une_strate_inconnues_sont_refusees_sans_invoquer_le_moteur()
    {
        var erreursQuestion = new StringWriter();
        Assert.Equal(2, IdentityCommand.Auditer(
            CheminOracle(), CheminRegistreReel(), "devine", "contenu", 1, new StringWriter(), erreursQuestion));
        Assert.Contains("question inconnue", erreursQuestion.ToString());

        var erreursStrate = new StringWriter();
        Assert.Equal(2, IdentityCommand.Auditer(
            CheminOracle(), CheminRegistreReel(), "conventions", "galaxie", 1, new StringWriter(), erreursStrate));
        Assert.Contains("strate inconnue", erreursStrate.ToString());
    }
}
