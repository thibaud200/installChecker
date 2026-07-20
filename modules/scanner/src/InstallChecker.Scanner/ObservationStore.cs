using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace InstallChecker;

/// <summary>Résultats bruts des capacités pour un fichier, sans interprétation métier.</summary>
public sealed record FileObservation(
    string Path, long Size, string Sha256, string ScannedAt,
    string MagicHex, string? Container,
    VersionInfoObservation VersionInfo,
    PeInfoExtractor.PeInfo PeInfo,
    AuthenticodeExtractor.AuthenticodeInfo Authenticode,
    MsiPropertiesExtractor.MsiProperties MsiProperties,
    AppxManifestExtractor.AppxManifest AppxManifest);

/// <summary>Déclaration historique d'un scan et du volume observé.</summary>
public sealed record ScanDeclaration(string VolumeId, string? VolumeLabel, string RootPath, string StartedAt, string? Extensions);

/// <summary>
/// Stocke une seule fois chaque snapshot brut immuable et une occurrence légère par chemin et par scan.
/// La transaction unique garantit qu'un scan interrompu ne laisse aucun état partiel.
/// </summary>
public sealed class ObservationStore : IDisposable
{
    public const long SchemaVersion = 3;

    private readonly SqliteConnection _connection;
    private readonly SqliteTransaction _transaction;
    private readonly long _scanId;
    private readonly SqliteCommand _selectSnapshot;
    private readonly SqliteCommand _insertSnapshot;
    private readonly SqliteCommand _insertDetails;
    private readonly SqliteCommand _insertEntry;
    private bool _commitEffectue;

    public ObservationStore(string dbPath, ScanDeclaration scan)
    {
        _connection = new SqliteConnection($"Data Source={dbPath}");
        try
        {
            _connection.Open();
            InitialiserSchema(dbPath);
        }
        catch
        {
            _connection.Dispose();
            throw;
        }

        _transaction = _connection.BeginTransaction();
        _scanId = InsererScan(scan);

        _selectSnapshot = CreerCommande("""
            SELECT id, canonical_payload
            FROM observation_snapshots
            WHERE snapshot_key = $snapshotKey;
            """, ("$snapshotKey", SqliteType.Text));

        _insertSnapshot = CreerCommande("""
            INSERT INTO observation_snapshots
                (snapshot_key, extraction_contract, canonical_payload, size, sha256)
            VALUES
                ($snapshotKey, $contract, $payload, $size, $sha256);
            SELECT last_insert_rowid();
            """,
            ("$snapshotKey", SqliteType.Text),
            ("$contract", SqliteType.Text),
            ("$payload", SqliteType.Blob),
            ("$size", SqliteType.Integer),
            ("$sha256", SqliteType.Text));

        _insertDetails = CreerCommande("""
            INSERT INTO snapshot_version_info
                (snapshot_id, product_name, company_name, product_version, file_version)
            VALUES ($snapshotId, $productName, $companyName, $productVersion, $fileVersion);

            INSERT INTO snapshot_file_headers
                (snapshot_id, magic_hex, container)
            VALUES ($snapshotId, $magicHex, $container);

            INSERT INTO snapshot_pe_info
                (snapshot_id, machine, subsystem, characteristics, timestamp, optional_header_magic)
            VALUES ($snapshotId, $machine, $subsystem, $characteristics, $timestamp, $optionalHeaderMagic);

            INSERT INTO snapshot_authenticode
                (snapshot_id, subject, issuer, serial_number, thumbprint, not_before, not_after)
            VALUES ($snapshotId, $subject, $issuer, $serialNumber, $thumbprint, $notBefore, $notAfter);

            INSERT INTO snapshot_msi_properties
                (snapshot_id, product_name, product_version, manufacturer, product_code, upgrade_code, product_language)
            VALUES ($snapshotId, $msiProductName, $msiProductVersion, $manufacturer, $productCode, $upgradeCode, $productLanguage);

            INSERT INTO snapshot_appx_manifest
                (snapshot_id, name, publisher, version, processor_architecture)
            VALUES ($snapshotId, $appxName, $appxPublisher, $appxVersion, $processorArchitecture);
            """,
            ("$snapshotId", SqliteType.Integer),
            ("$productName", SqliteType.Text),
            ("$companyName", SqliteType.Text),
            ("$productVersion", SqliteType.Text),
            ("$fileVersion", SqliteType.Text),
            ("$magicHex", SqliteType.Text),
            ("$container", SqliteType.Text),
            ("$machine", SqliteType.Text),
            ("$subsystem", SqliteType.Text),
            ("$characteristics", SqliteType.Integer),
            ("$timestamp", SqliteType.Integer),
            ("$optionalHeaderMagic", SqliteType.Text),
            ("$subject", SqliteType.Text),
            ("$issuer", SqliteType.Text),
            ("$serialNumber", SqliteType.Text),
            ("$thumbprint", SqliteType.Text),
            ("$notBefore", SqliteType.Text),
            ("$notAfter", SqliteType.Text),
            ("$msiProductName", SqliteType.Text),
            ("$msiProductVersion", SqliteType.Text),
            ("$manufacturer", SqliteType.Text),
            ("$productCode", SqliteType.Text),
            ("$upgradeCode", SqliteType.Text),
            ("$productLanguage", SqliteType.Text),
            ("$appxName", SqliteType.Text),
            ("$appxPublisher", SqliteType.Text),
            ("$appxVersion", SqliteType.Text),
            ("$processorArchitecture", SqliteType.Text));

        _insertEntry = CreerCommande("""
            INSERT INTO scan_entries (scan_id, snapshot_id, path, path_key, scanned_at)
            VALUES ($scanId, $snapshotId, $path, $pathKey, $scannedAt);
            SELECT last_insert_rowid();
            """,
            ("$scanId", SqliteType.Integer),
            ("$snapshotId", SqliteType.Integer),
            ("$path", SqliteType.Text),
            ("$pathKey", SqliteType.Text),
            ("$scannedAt", SqliteType.Text));
        _insertEntry.Parameters["$scanId"].Value = _scanId;
    }

    public long Persist(FileObservation observation)
    {
        var snapshot = CleSnapshotObservation.Calculer(observation);
        var snapshotId = TrouverSnapshot(snapshot);
        if (snapshotId is null)
            snapshotId = InsererSnapshot(observation, snapshot);

        Affecter(_insertEntry, "$snapshotId", snapshotId.Value);
        Affecter(_insertEntry, "$path", observation.Path);
        Affecter(_insertEntry, "$pathKey", CleChemin(observation.Path));
        Affecter(_insertEntry, "$scannedAt", observation.ScannedAt);
        return (long)_insertEntry.ExecuteScalar()!;
    }

    /// <summary>Projection JSON d'une observation déjà matérialisée.</summary>
    public string ProjectJson(FileObservation o, long observationId) =>
        JsonSerializer.Serialize(new
        {
            file = o.Path,
            observation_id = observationId,
            core = new
            {
                size = o.Size,
                sha256 = o.Sha256,
                scanned_at = o.ScannedAt,
                magic_hex = o.MagicHex,
                container = o.Container,
                pe_info = new
                {
                    machine = o.PeInfo.Machine,
                    subsystem = o.PeInfo.Subsystem,
                    characteristics = o.PeInfo.Characteristics,
                    timestamp = o.PeInfo.Timestamp,
                    optional_header_magic = o.PeInfo.OptionalHeaderMagic,
                },
            },
            metadata = new
            {
                version_info = new
                {
                    product_name = o.VersionInfo.ProductName,
                    company_name = o.VersionInfo.CompanyName,
                    product_version = o.VersionInfo.ProductVersion,
                    file_version = o.VersionInfo.FileVersion,
                },
                authenticode = new
                {
                    subject = o.Authenticode.Subject,
                    issuer = o.Authenticode.Issuer,
                    serial_number = o.Authenticode.SerialNumber,
                    thumbprint = o.Authenticode.Thumbprint,
                    not_before = o.Authenticode.NotBefore,
                    not_after = o.Authenticode.NotAfter,
                },
            },
            installer = new
            {
                msi_properties = new
                {
                    product_name = o.MsiProperties.ProductName,
                    product_version = o.MsiProperties.ProductVersion,
                    manufacturer = o.MsiProperties.Manufacturer,
                    product_code = o.MsiProperties.ProductCode,
                    upgrade_code = o.MsiProperties.UpgradeCode,
                    product_language = o.MsiProperties.ProductLanguage,
                },
                appx_manifest = new
                {
                    name = o.AppxManifest.Name,
                    publisher = o.AppxManifest.Publisher,
                    version = o.AppxManifest.Version,
                    processor_architecture = o.AppxManifest.ProcessorArchitecture,
                },
            },
            status = "ok",
        });

    /// <summary>Relit le scan courant depuis la base. Une transaction non validée n'est jamais exportable.</summary>
    public IEnumerable<string> ProjeterJsonDuScan()
    {
        if (!_commitEffectue)
            throw new InvalidOperationException("Le JSON d'un scan ne peut être projeté qu'après son commit.");

        using var commande = _connection.CreateCommand();
        commande.CommandText = """
            SELECT e.id, e.path, s.size, s.sha256, e.scanned_at,
                   h.magic_hex, h.container,
                   v.product_name, v.company_name, v.product_version, v.file_version,
                   p.machine, p.subsystem, p.characteristics, p.timestamp, p.optional_header_magic,
                   a.subject, a.issuer, a.serial_number, a.thumbprint, a.not_before, a.not_after,
                   m.product_name, m.product_version, m.manufacturer, m.product_code, m.upgrade_code, m.product_language,
                   x.name, x.publisher, x.version, x.processor_architecture
            FROM scan_entries e
            JOIN observation_snapshots s ON s.id = e.snapshot_id
            JOIN snapshot_file_headers h ON h.snapshot_id = s.id
            JOIN snapshot_version_info v ON v.snapshot_id = s.id
            JOIN snapshot_pe_info p ON p.snapshot_id = s.id
            JOIN snapshot_authenticode a ON a.snapshot_id = s.id
            JOIN snapshot_msi_properties m ON m.snapshot_id = s.id
            JOIN snapshot_appx_manifest x ON x.snapshot_id = s.id
            WHERE e.scan_id = $scanId
            ORDER BY e.id;
            """;
        commande.Parameters.AddWithValue("$scanId", _scanId);

        using var lecteur = commande.ExecuteReader();
        while (lecteur.Read())
        {
            var observation = new FileObservation(
                Path: lecteur.GetString(1),
                Size: lecteur.GetInt64(2),
                Sha256: lecteur.GetString(3),
                ScannedAt: lecteur.GetString(4),
                MagicHex: lecteur.GetString(5),
                Container: TexteNullable(lecteur, 6),
                VersionInfo: new VersionInfoObservation(
                    TexteNullable(lecteur, 7), TexteNullable(lecteur, 8),
                    TexteNullable(lecteur, 9), TexteNullable(lecteur, 10)),
                PeInfo: new PeInfoExtractor.PeInfo(
                    TexteNullable(lecteur, 11), TexteNullable(lecteur, 12),
                    EntierNullable(lecteur, 13), EntierNullable(lecteur, 14), TexteNullable(lecteur, 15)),
                Authenticode: new AuthenticodeExtractor.AuthenticodeInfo(
                    TexteNullable(lecteur, 16), TexteNullable(lecteur, 17), TexteNullable(lecteur, 18),
                    TexteNullable(lecteur, 19), TexteNullable(lecteur, 20), TexteNullable(lecteur, 21)),
                MsiProperties: new MsiPropertiesExtractor.MsiProperties(
                    TexteNullable(lecteur, 22), TexteNullable(lecteur, 23), TexteNullable(lecteur, 24),
                    TexteNullable(lecteur, 25), TexteNullable(lecteur, 26), TexteNullable(lecteur, 27)),
                AppxManifest: new AppxManifestExtractor.AppxManifest(
                    TexteNullable(lecteur, 28), TexteNullable(lecteur, 29),
                    TexteNullable(lecteur, 30), TexteNullable(lecteur, 31)));

            yield return ProjectJson(observation, lecteur.GetInt64(0));
        }
    }

    public void Commit()
    {
        _transaction.Commit();
        _commitEffectue = true;
    }

    public void Dispose()
    {
        _selectSnapshot.Dispose();
        _insertSnapshot.Dispose();
        _insertDetails.Dispose();
        _insertEntry.Dispose();
        _transaction.Dispose();
        _connection.Dispose();
    }

    private void InitialiserSchema(string dbPath)
    {
        using var readVersion = _connection.CreateCommand();
        readVersion.CommandText = "PRAGMA user_version;";
        var userVersion = (long)readVersion.ExecuteScalar()!;
        if (userVersion != 0 && userVersion != SchemaVersion)
            throw new InvalidDataException($"Erreur : base incompatible : {dbPath} : user_version={userVersion}, attendu {SchemaVersion}");

        using var create = _connection.CreateCommand();
        create.CommandText = """
            PRAGMA foreign_keys = ON;
            CREATE TABLE IF NOT EXISTS scans (
                id           INTEGER PRIMARY KEY AUTOINCREMENT,
                volume_id    TEXT NOT NULL,
                volume_label TEXT,
                root_path    TEXT NOT NULL,
                started_at   TEXT NOT NULL,
                extensions   TEXT
            );
            CREATE TABLE IF NOT EXISTS observation_snapshots (
                id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                snapshot_key        TEXT NOT NULL UNIQUE,
                extraction_contract TEXT NOT NULL,
                canonical_payload   BLOB NOT NULL,
                size                INTEGER NOT NULL,
                sha256              TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS snapshot_version_info (
                snapshot_id    INTEGER PRIMARY KEY REFERENCES observation_snapshots(id),
                product_name   TEXT,
                company_name   TEXT,
                product_version TEXT,
                file_version   TEXT
            );
            CREATE TABLE IF NOT EXISTS snapshot_file_headers (
                snapshot_id INTEGER PRIMARY KEY REFERENCES observation_snapshots(id),
                magic_hex   TEXT NOT NULL,
                container   TEXT
            );
            CREATE TABLE IF NOT EXISTS snapshot_pe_info (
                snapshot_id           INTEGER PRIMARY KEY REFERENCES observation_snapshots(id),
                machine               TEXT,
                subsystem             TEXT,
                characteristics       INTEGER,
                timestamp             INTEGER,
                optional_header_magic TEXT
            );
            CREATE TABLE IF NOT EXISTS snapshot_authenticode (
                snapshot_id  INTEGER PRIMARY KEY REFERENCES observation_snapshots(id),
                subject      TEXT,
                issuer       TEXT,
                serial_number TEXT,
                thumbprint   TEXT,
                not_before   TEXT,
                not_after    TEXT
            );
            CREATE TABLE IF NOT EXISTS snapshot_msi_properties (
                snapshot_id     INTEGER PRIMARY KEY REFERENCES observation_snapshots(id),
                product_name    TEXT,
                product_version TEXT,
                manufacturer    TEXT,
                product_code    TEXT,
                upgrade_code    TEXT,
                product_language TEXT
            );
            CREATE TABLE IF NOT EXISTS snapshot_appx_manifest (
                snapshot_id           INTEGER PRIMARY KEY REFERENCES observation_snapshots(id),
                name                  TEXT,
                publisher             TEXT,
                version               TEXT,
                processor_architecture TEXT
            );
            CREATE TABLE IF NOT EXISTS scan_entries (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                scan_id     INTEGER NOT NULL REFERENCES scans(id),
                snapshot_id INTEGER NOT NULL REFERENCES observation_snapshots(id),
                path        TEXT NOT NULL,
                path_key    TEXT NOT NULL,
                scanned_at  TEXT NOT NULL,
                UNIQUE(scan_id, path_key)
            );
            CREATE INDEX IF NOT EXISTS ix_scans_volume ON scans(volume_id, id DESC);
            CREATE INDEX IF NOT EXISTS ix_scan_entries_scan ON scan_entries(scan_id);
            CREATE INDEX IF NOT EXISTS ix_scan_entries_snapshot ON scan_entries(snapshot_id);

            CREATE VIEW IF NOT EXISTS scan_observations AS
            SELECT e.id, e.scan_id, e.path, s.size, s.sha256, e.scanned_at
            FROM scan_entries e
            JOIN observation_snapshots s ON s.id = e.snapshot_id;

            CREATE VIEW IF NOT EXISTS version_info AS
            SELECT e.id AS observation_id, d.product_name, d.company_name, d.product_version, d.file_version
            FROM scan_entries e
            JOIN snapshot_version_info d ON d.snapshot_id = e.snapshot_id;

            CREATE VIEW IF NOT EXISTS file_headers AS
            SELECT e.id AS observation_id, d.magic_hex, d.container
            FROM scan_entries e
            JOIN snapshot_file_headers d ON d.snapshot_id = e.snapshot_id;

            CREATE VIEW IF NOT EXISTS pe_info AS
            SELECT e.id AS observation_id, d.machine, d.subsystem, d.characteristics, d.timestamp, d.optional_header_magic
            FROM scan_entries e
            JOIN snapshot_pe_info d ON d.snapshot_id = e.snapshot_id;

            CREATE VIEW IF NOT EXISTS authenticode AS
            SELECT e.id AS observation_id, d.subject, d.issuer, d.serial_number, d.thumbprint, d.not_before, d.not_after
            FROM scan_entries e
            JOIN snapshot_authenticode d ON d.snapshot_id = e.snapshot_id;

            CREATE VIEW IF NOT EXISTS msi_properties AS
            SELECT e.id AS observation_id, d.product_name, d.product_version, d.manufacturer,
                   d.product_code, d.upgrade_code, d.product_language
            FROM scan_entries e
            JOIN snapshot_msi_properties d ON d.snapshot_id = e.snapshot_id;

            CREATE VIEW IF NOT EXISTS appx_manifest AS
            SELECT e.id AS observation_id, d.name, d.publisher, d.version, d.processor_architecture
            FROM scan_entries e
            JOIN snapshot_appx_manifest d ON d.snapshot_id = e.snapshot_id;
            """;
        create.ExecuteNonQuery();

        if (userVersion == 0)
        {
            using var stamp = _connection.CreateCommand();
            stamp.CommandText = $"PRAGMA user_version = {SchemaVersion};";
            stamp.ExecuteNonQuery();
        }
    }

    private long InsererScan(ScanDeclaration scan)
    {
        using var commande = _connection.CreateCommand();
        commande.Transaction = _transaction;
        commande.CommandText = """
            INSERT INTO scans (volume_id, volume_label, root_path, started_at, extensions)
            VALUES ($volumeId, $volumeLabel, $rootPath, $startedAt, $extensions);
            SELECT last_insert_rowid();
            """;
        commande.Parameters.AddWithValue("$volumeId", scan.VolumeId);
        commande.Parameters.AddWithValue("$volumeLabel", (object?)scan.VolumeLabel ?? DBNull.Value);
        commande.Parameters.AddWithValue("$rootPath", scan.RootPath);
        commande.Parameters.AddWithValue("$startedAt", scan.StartedAt);
        commande.Parameters.AddWithValue("$extensions", (object?)scan.Extensions ?? DBNull.Value);
        return (long)commande.ExecuteScalar()!;
    }

    private long? TrouverSnapshot(SnapshotCalcule snapshot)
    {
        Affecter(_selectSnapshot, "$snapshotKey", snapshot.Cle);
        using var lecteur = _selectSnapshot.ExecuteReader();
        if (!lecteur.Read())
            return null;

        var payloadExistant = lecteur.GetFieldValue<byte[]>(1);
        if (!payloadExistant.AsSpan().SequenceEqual(snapshot.ChargeCanonique))
            throw new InvalidDataException($"Collision de clé de snapshot détectée : {snapshot.Cle}");
        return lecteur.GetInt64(0);
    }

    private long InsererSnapshot(FileObservation o, SnapshotCalcule snapshot)
    {
        Affecter(_insertSnapshot, "$snapshotKey", snapshot.Cle);
        Affecter(_insertSnapshot, "$contract", CleSnapshotObservation.VersionContrat);
        Affecter(_insertSnapshot, "$payload", snapshot.ChargeCanonique);
        Affecter(_insertSnapshot, "$size", o.Size);
        Affecter(_insertSnapshot, "$sha256", o.Sha256);
        var snapshotId = (long)_insertSnapshot.ExecuteScalar()!;

        Affecter(_insertDetails, "$snapshotId", snapshotId);
        Affecter(_insertDetails, "$productName", o.VersionInfo.ProductName);
        Affecter(_insertDetails, "$companyName", o.VersionInfo.CompanyName);
        Affecter(_insertDetails, "$productVersion", o.VersionInfo.ProductVersion);
        Affecter(_insertDetails, "$fileVersion", o.VersionInfo.FileVersion);
        Affecter(_insertDetails, "$magicHex", o.MagicHex);
        Affecter(_insertDetails, "$container", o.Container);
        Affecter(_insertDetails, "$machine", o.PeInfo.Machine);
        Affecter(_insertDetails, "$subsystem", o.PeInfo.Subsystem);
        Affecter(_insertDetails, "$characteristics", o.PeInfo.Characteristics);
        Affecter(_insertDetails, "$timestamp", o.PeInfo.Timestamp);
        Affecter(_insertDetails, "$optionalHeaderMagic", o.PeInfo.OptionalHeaderMagic);
        Affecter(_insertDetails, "$subject", o.Authenticode.Subject);
        Affecter(_insertDetails, "$issuer", o.Authenticode.Issuer);
        Affecter(_insertDetails, "$serialNumber", o.Authenticode.SerialNumber);
        Affecter(_insertDetails, "$thumbprint", o.Authenticode.Thumbprint);
        Affecter(_insertDetails, "$notBefore", o.Authenticode.NotBefore);
        Affecter(_insertDetails, "$notAfter", o.Authenticode.NotAfter);
        Affecter(_insertDetails, "$msiProductName", o.MsiProperties.ProductName);
        Affecter(_insertDetails, "$msiProductVersion", o.MsiProperties.ProductVersion);
        Affecter(_insertDetails, "$manufacturer", o.MsiProperties.Manufacturer);
        Affecter(_insertDetails, "$productCode", o.MsiProperties.ProductCode);
        Affecter(_insertDetails, "$upgradeCode", o.MsiProperties.UpgradeCode);
        Affecter(_insertDetails, "$productLanguage", o.MsiProperties.ProductLanguage);
        Affecter(_insertDetails, "$appxName", o.AppxManifest.Name);
        Affecter(_insertDetails, "$appxPublisher", o.AppxManifest.Publisher);
        Affecter(_insertDetails, "$appxVersion", o.AppxManifest.Version);
        Affecter(_insertDetails, "$processorArchitecture", o.AppxManifest.ProcessorArchitecture);
        _insertDetails.ExecuteNonQuery();

        return snapshotId;
    }

    private SqliteCommand CreerCommande(string sql, params (string Nom, SqliteType Type)[] parametres)
    {
        var commande = _connection.CreateCommand();
        commande.Transaction = _transaction;
        commande.CommandText = sql;
        foreach (var (nom, type) in parametres)
            commande.Parameters.Add(nom, type);
        commande.Prepare();
        return commande;
    }

    private static void Affecter(SqliteCommand commande, string nom, object? valeur) =>
        commande.Parameters[nom].Value = valeur ?? DBNull.Value;

    private static string? TexteNullable(SqliteDataReader lecteur, int ordinal) =>
        lecteur.IsDBNull(ordinal) ? null : lecteur.GetString(ordinal);

    private static long? EntierNullable(SqliteDataReader lecteur, int ordinal) =>
        lecteur.IsDBNull(ordinal) ? null : lecteur.GetInt64(ordinal);

    private static string CleChemin(string path) =>
        Path.GetFullPath(path).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToUpperInvariant();
}
