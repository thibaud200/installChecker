using InstallChecker;
using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Observations;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Identity.Tests;

public class LecteurDObservationsSqliteTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "identity-omega-tests-" + Guid.NewGuid())).FullName;

    public void Dispose()
    {
        SqliteConnection.ClearAllPools(); // libère les handles natifs mis en cache avant suppression du dossier
        Directory.Delete(_root, recursive: true);
    }

    private string NouveauCheminDeBase() => Path.Combine(_root, Guid.NewGuid() + ".db");

    private static string CheminOracle()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        var racine = repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable");
        return Path.Combine(racine, "tests", "oracle", "corpus1-postA1.db");
    }

    private string ScannerDossier(string dossier)
    {
        var db = NouveauCheminDeBase();
        var exitCode = ScanCommand.Run(dossier, db, jsonOutput: false, TextWriter.Null, TextWriter.Null);
        Assert.Equal(0, exitCode);
        return db;
    }

    private static IEnumerable<KeyValuePair<Attribut, ValeurObservee>> Trier(ActeObservation acte) =>
        acte.Attributs.OrderBy(kv => kv.Key.Capacite, StringComparer.Ordinal).ThenBy(kv => kv.Key.Nom, StringComparer.Ordinal);

    // --- Oracle corpus1-postA1.db (013 § 11, 014 § 10 É4) ---

    [Fact]
    public void Lit_loracle_complet_497_actes()
    {
        var modele = new LecteurDObservationsSqlite(CheminOracle()).ProjeterModele();

        Assert.Equal(497, modele.Actes.Count);
        Assert.All(modele.Actes, acte =>
        {
            Assert.True(acte.Taille >= 0);
            Assert.False(string.IsNullOrEmpty(acte.Empreinte));
            Assert.NotEmpty(acte.Attributs);
        });
    }

    [Fact]
    public void Deux_lectures_de_loracle_produisent_le_meme_modele()
    {
        var premiere = new LecteurDObservationsSqlite(CheminOracle()).ProjeterModele();
        var seconde = new LecteurDObservationsSqlite(CheminOracle()).ProjeterModele();

        Assert.Equal(premiere.Actes.Count, seconde.Actes.Count);
        foreach (var (a, b) in premiere.Actes.Zip(seconde.Actes))
        {
            Assert.Equal(a.Identifiant, b.Identifiant);
            Assert.Equal(a.Taille, b.Taille);
            Assert.Equal(a.Empreinte, b.Empreinte);
            Assert.Equal(Trier(a).Select(kv => (kv.Key, kv.Value)), Trier(b).Select(kv => (kv.Key, kv.Value)));
        }
    }

    // --- Fidélité de projection ---

    [Fact]
    public void Projette_fidelement_les_valeurs_absentes_dun_fichier_sans_capacite_reconnue()
    {
        var fichier = Path.Combine(_root, "sans-capacite.txt");
        File.WriteAllText(fichier, "aucune capacité reconnue ici");

        var modele = new LecteurDObservationsSqlite(ScannerDossier(_root)).ProjeterModele();
        var acte = Assert.Single(modele.Actes);

        Assert.Equal(ValeurObservee.Absente.Instance, acte.Attributs[new Attribut("pe_info", "machine")]);
        Assert.Equal(ValeurObservee.Absente.Instance, acte.Attributs[new Attribut("authenticode", "subject")]);
        Assert.Equal(ValeurObservee.Absente.Instance, acte.Attributs[new Attribut("version_info", "product_name")]);
        Assert.Equal(ValeurObservee.Absente.Instance, acte.Attributs[new Attribut("file_headers", "container")]);
    }

    [Fact]
    public void Projette_fidelement_les_valeurs_presentes_dun_fichier_PE()
    {
        var copie = Path.Combine(_root, "kernel32.dll");
        File.Copy(Path.Combine(Environment.SystemDirectory, "kernel32.dll"), copie);

        var modele = new LecteurDObservationsSqlite(ScannerDossier(_root)).ProjeterModele();
        var acte = Assert.Single(modele.Actes);

        Assert.Equal(new FileInfo(copie).Length, acte.Taille);
        Assert.Equal(new ValeurObservee.Texte("pe"), acte.Attributs[new Attribut("file_headers", "container")]);
        var magicHex = Assert.IsType<ValeurObservee.Texte>(acte.Attributs[new Attribut("file_headers", "magic_hex")]);
        Assert.StartsWith("4d5a", magicHex.Valeur);
        Assert.IsType<ValeurObservee.Texte>(acte.Attributs[new Attribut("version_info", "company_name")]);
    }

    // --- Contexte : canal séparé (A1) ---

    [Fact]
    public void Le_contexte_porte_le_chemin_et_la_date_hors_du_modele()
    {
        var fichier = Path.Combine(_root, "a.txt");
        File.WriteAllText(fichier, "x");
        var db = ScannerDossier(_root);
        var lecteur = new LecteurDObservationsSqlite(db);

        var contexte = Assert.Single(lecteur.ProjeterContexte());
        Assert.Equal(fichier, contexte.Chemin);
        Assert.False(string.IsNullOrEmpty(contexte.DateDeScan));

        var acte = Assert.Single(lecteur.ProjeterModele().Actes);
        Assert.Equal(contexte.Identifiant, acte.Identifiant);
    }

    // --- Indépendance de l'ordre de stockage SQL ---

    [Fact]
    public void Lordre_de_stockage_SQL_nimporte_pas_a_lordre_logique_des_actes()
    {
        var chemin = NouveauCheminDeBase();
        using (var store = new ObservationStore(chemin)) store.Commit(); // crée le schéma, stamp user_version = 1

        using (var connection = new SqliteConnection($"Data Source={chemin}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            commande.CommandText = """
                INSERT INTO scan_observations (id, path, size, sha256, scanned_at) VALUES
                    (30, 'c.txt', 3, 'sha-c', '2026-01-01T00:00:00Z'),
                    (10, 'a.txt', 1, 'sha-a', '2026-01-01T00:00:00Z'),
                    (20, 'b.txt', 2, 'sha-b', '2026-01-01T00:00:00Z');
                INSERT INTO version_info (observation_id) VALUES (30), (10), (20);
                INSERT INTO file_headers (observation_id, magic_hex) VALUES (30, 'aa'), (10, 'aa'), (20, 'aa');
                INSERT INTO pe_info (observation_id) VALUES (30), (10), (20);
                INSERT INTO authenticode (observation_id) VALUES (30), (10), (20);
                INSERT INTO msi_properties (observation_id) VALUES (30), (10), (20);
                INSERT INTO appx_manifest (observation_id) VALUES (30), (10), (20);
                """;
            commande.ExecuteNonQuery();
        }

        var modele = new LecteurDObservationsSqlite(chemin).ProjeterModele();

        Assert.Equal([10, 20, 30], modele.Actes.Select(a => a.Identifiant));
    }

    // --- Refus nommés (011 § 5, 014 C1) ---

    [Fact]
    public void Support_absent_est_refuse_nommement()
    {
        Assert.Throws<OmegaAbsentException>(
            () => new LecteurDObservationsSqlite(NouveauCheminDeBase()).ProjeterModele());
    }

    [Fact]
    public void User_version_incorrect_est_refuse_comme_incompatible()
    {
        var chemin = NouveauCheminDeBase();
        using (var store = new ObservationStore(chemin)) store.Commit();

        using (var connection = new SqliteConnection($"Data Source={chemin}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            commande.CommandText = "PRAGMA user_version = 2;";
            commande.ExecuteNonQuery();
        }

        Assert.Throws<OmegaIncompatibleException>(
            () => new LecteurDObservationsSqlite(chemin).ProjeterModele());
    }

    [Fact]
    public void Schema_incompatible_est_refuse_comme_invalide()
    {
        var chemin = NouveauCheminDeBase();
        using (var connection = new SqliteConnection($"Data Source={chemin}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            commande.CommandText = "CREATE TABLE autre_chose (x INTEGER); PRAGMA user_version = 1;";
            commande.ExecuteNonQuery();
        }

        Assert.Throws<OmegaInvalideException>(
            () => new LecteurDObservationsSqlite(chemin).ProjeterModele());
    }

    [Fact]
    public void Ligne_de_capacite_manquante_rompt_linvariant_1_1()
    {
        var chemin = NouveauCheminDeBase();
        using (var store = new ObservationStore(chemin)) store.Commit();

        using (var connection = new SqliteConnection($"Data Source={chemin}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            // Un acte dans scan_observations, mais aucune ligne pe_info correspondante : 1:1 rompu.
            commande.CommandText = """
                INSERT INTO scan_observations (id, path, size, sha256, scanned_at)
                VALUES (1, 'a.txt', 1, 'sha-a', '2026-01-01T00:00:00Z');
                INSERT INTO version_info (observation_id) VALUES (1);
                INSERT INTO file_headers (observation_id, magic_hex) VALUES (1, 'aa');
                INSERT INTO authenticode (observation_id) VALUES (1);
                INSERT INTO msi_properties (observation_id) VALUES (1);
                INSERT INTO appx_manifest (observation_id) VALUES (1);
                """;
            commande.ExecuteNonQuery();
        }

        Assert.Throws<OmegaInvalideException>(
            () => new LecteurDObservationsSqlite(chemin).ProjeterModele());
    }

    // --- D2 (audit final) : aucune exception .NET ne doit fuiter sur une colonne obligatoire absente ---

    [Fact]
    public void Sha256_absent_dans_scan_observations_est_refuse_comme_invalide()
    {
        var chemin = NouveauCheminDeBase();
        using (var connection = new SqliteConnection($"Data Source={chemin}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            commande.CommandText = """
                CREATE TABLE scan_observations (id INTEGER PRIMARY KEY, path TEXT, size INTEGER, sha256 TEXT, scanned_at TEXT);
                INSERT INTO scan_observations (id, path, size, sha256, scanned_at) VALUES (1, 'a.txt', 1, NULL, '2026-01-01T00:00:00Z');
                PRAGMA user_version = 1;
                """;
            commande.ExecuteNonQuery();
        }

        Assert.Throws<OmegaInvalideException>(
            () => new LecteurDObservationsSqlite(chemin).ProjeterModele());
    }

    [Fact]
    public void Size_absent_dans_scan_observations_est_refuse_comme_invalide()
    {
        var chemin = NouveauCheminDeBase();
        using (var connection = new SqliteConnection($"Data Source={chemin}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            commande.CommandText = """
                CREATE TABLE scan_observations (id INTEGER PRIMARY KEY, path TEXT, size INTEGER, sha256 TEXT, scanned_at TEXT);
                INSERT INTO scan_observations (id, path, size, sha256, scanned_at) VALUES (1, 'a.txt', NULL, 'sha-a', '2026-01-01T00:00:00Z');
                PRAGMA user_version = 1;
                """;
            commande.ExecuteNonQuery();
        }

        Assert.Throws<OmegaInvalideException>(
            () => new LecteurDObservationsSqlite(chemin).ProjeterModele());
    }

    [Fact]
    public void Path_absent_dans_scan_observations_est_refuse_comme_invalide()
    {
        var chemin = NouveauCheminDeBase();
        using (var connection = new SqliteConnection($"Data Source={chemin}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            commande.CommandText = """
                CREATE TABLE scan_observations (id INTEGER PRIMARY KEY, path TEXT, size INTEGER, sha256 TEXT, scanned_at TEXT);
                INSERT INTO scan_observations (id, path, size, sha256, scanned_at) VALUES (1, NULL, 1, 'sha-a', '2026-01-01T00:00:00Z');
                PRAGMA user_version = 1;
                """;
            commande.ExecuteNonQuery();
        }

        Assert.Throws<OmegaInvalideException>(
            () => new LecteurDObservationsSqlite(chemin).ProjeterContexte());
    }

    [Fact]
    public void Scanned_at_absent_dans_scan_observations_est_refuse_comme_invalide()
    {
        var chemin = NouveauCheminDeBase();
        using (var connection = new SqliteConnection($"Data Source={chemin}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            commande.CommandText = """
                CREATE TABLE scan_observations (id INTEGER PRIMARY KEY, path TEXT, size INTEGER, sha256 TEXT, scanned_at TEXT);
                INSERT INTO scan_observations (id, path, size, sha256, scanned_at) VALUES (1, 'a.txt', 1, 'sha-a', NULL);
                PRAGMA user_version = 1;
                """;
            commande.ExecuteNonQuery();
        }

        Assert.Throws<OmegaInvalideException>(
            () => new LecteurDObservationsSqlite(chemin).ProjeterContexte());
    }

    // --- D5 (audit final) : fichier présent mais non-SQLite → refusé comme absent, jamais une exception .NET ---

    [Fact]
    public void Fichier_present_mais_non_sqlite_est_refuse_comme_absent()
    {
        var chemin = NouveauCheminDeBase();
        File.WriteAllBytes(chemin, "ceci n'est pas une base SQLite"u8.ToArray());

        Assert.Throws<OmegaAbsentException>(
            () => new LecteurDObservationsSqlite(chemin).ProjeterModele());
    }

    // --- Substituabilité du port (I42) ---

    private static int CompterActes(IObservationsSource source) => source.ProjeterModele().Actes.Count;

    [Fact]
    public void Ladaptateur_memoire_est_substituable_au_lecteur_SQLite()
    {
        var fichier = Path.Combine(_root, "a.txt");
        File.WriteAllText(fichier, "x");
        var sqlite = new LecteurDObservationsSqlite(ScannerDossier(_root));

        var modele = sqlite.ProjeterModele();
        var contexte = sqlite.ProjeterContexte();
        var memoire = new SourceObservationsEnMemoire(modele, contexte);

        Assert.Equal(CompterActes(sqlite), CompterActes(memoire));
        Assert.Same(modele, memoire.ProjeterModele());
        Assert.Same(contexte, memoire.ProjeterContexte());
    }
}
