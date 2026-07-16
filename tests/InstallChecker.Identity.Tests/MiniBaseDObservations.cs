using Microsoft.Data.Sqlite;

namespace InstallChecker.Identity.Tests;

/// <summary>
/// Fabrique de supports d'observations conformes au contrat <c>user_version = 1</c> (014 § 6),
/// pour les tests du moteur — sans invoquer le producteur. La frontière entre le moteur et le
/// pipeline est « une frontière de données, pas de code » (013 § 2) : le contrat entre eux est le
/// schéma documenté, que ce DDL matérialise côté fixtures. C'est le prix, assumé, de l'autonomie
/// de la suite Identity (016 § 4.2, report 10 — résorbé au jalon V3-1) : la suite fabrique ses
/// supports depuis le contrat, jamais depuis le producteur.
/// </summary>
internal static class MiniBaseDObservations
{
    public static void CreerConforme(string chemin)
    {
        using var connection = new SqliteConnection($"Data Source={chemin}");
        connection.Open();
        using var commande = connection.CreateCommand();
        commande.CommandText = """
            CREATE TABLE scan_observations (
                id         INTEGER PRIMARY KEY AUTOINCREMENT,
                path       TEXT NOT NULL,
                size       INTEGER NOT NULL,
                sha256     TEXT NOT NULL,
                scanned_at TEXT NOT NULL
            );
            CREATE TABLE version_info (
                observation_id  INTEGER NOT NULL,
                product_name    TEXT,
                company_name    TEXT,
                product_version TEXT,
                file_version    TEXT
            );
            CREATE TABLE file_headers (
                observation_id INTEGER NOT NULL,
                magic_hex      TEXT NOT NULL,
                container      TEXT
            );
            CREATE TABLE pe_info (
                observation_id        INTEGER NOT NULL,
                machine               TEXT,
                subsystem             TEXT,
                characteristics       INTEGER,
                timestamp             INTEGER,
                optional_header_magic TEXT
            );
            CREATE TABLE authenticode (
                observation_id INTEGER NOT NULL,
                subject        TEXT,
                issuer         TEXT,
                serial_number  TEXT,
                thumbprint     TEXT,
                not_before     TEXT,
                not_after      TEXT
            );
            CREATE TABLE msi_properties (
                observation_id   INTEGER NOT NULL,
                product_name     TEXT,
                product_version  TEXT,
                manufacturer     TEXT,
                product_code     TEXT,
                upgrade_code     TEXT,
                product_language TEXT
            );
            CREATE TABLE appx_manifest (
                observation_id         INTEGER NOT NULL,
                name                   TEXT,
                publisher              TEXT,
                version                TEXT,
                processor_architecture TEXT
            );
            PRAGMA user_version = 1;
            """;
        commande.ExecuteNonQuery();
    }

    /// <summary>
    /// Fabrique du contrat v2 (spec multi-disque D2) : mêmes tables de capacité, plus la table
    /// <c>scans</c> et la colonne <c>scan_id</c> — le schéma que le producteur écrit à partir de
    /// la gestion multi-disque, matérialisé côté fixtures comme <see cref="CreerConforme"/> pour v1.
    /// </summary>
    public static void CreerConformeV2(string chemin)
    {
        using var connection = new SqliteConnection($"Data Source={chemin}");
        connection.Open();
        using var commande = connection.CreateCommand();
        commande.CommandText = """
            CREATE TABLE scans (
                id           INTEGER PRIMARY KEY AUTOINCREMENT,
                volume_id    TEXT NOT NULL,
                volume_label TEXT,
                root_path    TEXT NOT NULL,
                started_at   TEXT NOT NULL,
                extensions   TEXT
            );
            CREATE TABLE scan_observations (
                id         INTEGER PRIMARY KEY AUTOINCREMENT,
                scan_id    INTEGER NOT NULL,
                path       TEXT NOT NULL,
                size       INTEGER NOT NULL,
                sha256     TEXT NOT NULL,
                scanned_at TEXT NOT NULL
            );
            CREATE TABLE version_info (
                observation_id  INTEGER NOT NULL,
                product_name    TEXT,
                company_name    TEXT,
                product_version TEXT,
                file_version    TEXT
            );
            CREATE TABLE file_headers (
                observation_id INTEGER NOT NULL,
                magic_hex      TEXT NOT NULL,
                container      TEXT
            );
            CREATE TABLE pe_info (
                observation_id        INTEGER NOT NULL,
                machine               TEXT,
                subsystem             TEXT,
                characteristics       INTEGER,
                timestamp             INTEGER,
                optional_header_magic TEXT
            );
            CREATE TABLE authenticode (
                observation_id INTEGER NOT NULL,
                subject        TEXT,
                issuer         TEXT,
                serial_number  TEXT,
                thumbprint     TEXT,
                not_before     TEXT,
                not_after      TEXT
            );
            CREATE TABLE msi_properties (
                observation_id   INTEGER NOT NULL,
                product_name     TEXT,
                product_version  TEXT,
                manufacturer     TEXT,
                product_code     TEXT,
                upgrade_code     TEXT,
                product_language TEXT
            );
            CREATE TABLE appx_manifest (
                observation_id         INTEGER NOT NULL,
                name                   TEXT,
                publisher              TEXT,
                version                TEXT,
                processor_architecture TEXT
            );
            PRAGMA user_version = 2;
            """;
        commande.ExecuteNonQuery();
    }
}
