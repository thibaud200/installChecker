using System.Diagnostics;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace InstallChecker;

/// <summary>
/// Résultats bruts des capacités pour un fichier, tels que retournés par les extracteurs.
/// Simple objet de transmission entre le scan et le stockage — aucune logique.
/// </summary>
public sealed record FileObservation(
    string Path, long Size, string Sha256, string ScannedAt,
    string MagicHex, string? Container,
    FileVersionInfo VersionInfo,
    PeInfoExtractor.PeInfo PeInfo,
    AuthenticodeExtractor.AuthenticodeInfo Authenticode,
    MsiPropertiesExtractor.MsiProperties MsiProperties,
    AppxManifestExtractor.AppxManifest AppxManifest);

/// <summary>
/// La déclaration du scan en cours (spec multi-disque D2/D3) : l'identité observée du volume,
/// la racine et le filtre d'extensions tels que passés — conservés pour que l'éviction de l'état
/// précédent du volume reste explicable. Simple objet de transmission — aucune logique.
/// </summary>
public sealed record ScanDeclaration(string VolumeId, string? VolumeLabel, string RootPath, string StartedAt, string? Extensions);

/// <summary>
/// Propriétaire unique du stockage : création de la base, schéma, écriture des observations
/// et projection JSON. Les tables sont append-only : chaque exécution ajoute de nouvelles
/// observations, sans dédoublonnage. La projection JSON est dérivée strictement des mêmes
/// valeurs que les INSERT (zéro recalcul).
/// </summary>
public sealed class ObservationStore : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SqliteTransaction _transaction;

    private readonly SqliteCommand _insert;
    private readonly SqliteParameter _pPath, _pSize, _pSha256, _pScannedAt;

    private readonly SqliteCommand _insertVersion;
    private readonly SqliteParameter _pObservationId, _pProductName, _pCompanyName, _pProductVersion, _pFileVersion;

    private readonly SqliteCommand _insertHeader;
    private readonly SqliteParameter _pHeaderObservationId, _pMagicHex, _pContainer;

    private readonly SqliteCommand _insertPe;
    private readonly SqliteParameter _pPeObservationId, _pMachine, _pSubsystem, _pCharacteristics, _pTimestamp, _pOptionalHeaderMagic;

    private readonly SqliteCommand _insertAuthenticode;
    private readonly SqliteParameter _pAuthObservationId, _pSubject, _pIssuer, _pSerialNumber, _pThumbprint, _pNotBefore, _pNotAfter;

    private readonly SqliteCommand _insertMsi;
    private readonly SqliteParameter _pMsiObservationId, _pMsiProductName, _pMsiProductVersion, _pMsiManufacturer, _pMsiProductCode, _pMsiUpgradeCode, _pMsiProductLanguage;

    private readonly SqliteCommand _insertAppx;
    private readonly SqliteParameter _pAppxObservationId, _pAppxName, _pAppxPublisher, _pAppxVersion, _pAppxProcessorArchitecture;

    /// <summary>Version de schéma écrite et exigée dans PRAGMA user_version. Aucune migration : version inconnue = erreur.</summary>
    public const long SchemaVersion = 2;

    private readonly long _scanId;

    /// <summary>
    /// Ouvre (ou crée) la base, crée le schéma si absent, ouvre la transaction unique du scan,
    /// enregistre la déclaration du scan et prépare les INSERT. Lève <see cref="SqliteException"/>
    /// si la base est inaccessible, <see cref="InvalidDataException"/> si son user_version n'est
    /// pas celui attendu.
    /// </summary>
    public ObservationStore(string dbPath, ScanDeclaration scan)
    {
        _connection = new SqliteConnection($"Data Source={dbPath}");
        try
        {
            _connection.Open(); // crée le fichier si absent

            using var readVersion = _connection.CreateCommand();
            readVersion.CommandText = "PRAGMA user_version;";
            var userVersion = (long)readVersion.ExecuteScalar()!;
            // 0 = base neuve (valeur SQLite par défaut) : on initialise. Toute autre valeur que
            // SchemaVersion = base d'un autre monde : erreur explicite, aucune migration.
            if (userVersion != 0 && userVersion != SchemaVersion)
                throw new InvalidDataException($"Erreur : base incompatible : {dbPath} : user_version={userVersion}, attendu {SchemaVersion}");

            using var create = _connection.CreateCommand();
            create.CommandText = """
                CREATE TABLE IF NOT EXISTS scans (
                    id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    volume_id    TEXT NOT NULL,
                    volume_label TEXT,
                    root_path    TEXT NOT NULL,
                    started_at   TEXT NOT NULL,
                    extensions   TEXT
                );
                CREATE TABLE IF NOT EXISTS scan_observations (
                    id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    scan_id    INTEGER NOT NULL,
                    path       TEXT NOT NULL,
                    size       INTEGER NOT NULL,
                    sha256     TEXT NOT NULL,
                    scanned_at TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS version_info (
                    observation_id  INTEGER NOT NULL,
                    product_name    TEXT,
                    company_name    TEXT,
                    product_version TEXT,
                    file_version    TEXT
                );
                CREATE TABLE IF NOT EXISTS file_headers (
                    observation_id INTEGER NOT NULL,
                    magic_hex      TEXT NOT NULL,
                    container      TEXT
                );
                CREATE TABLE IF NOT EXISTS pe_info (
                    observation_id        INTEGER NOT NULL,
                    machine               TEXT,
                    subsystem             TEXT,
                    characteristics       INTEGER,
                    timestamp             INTEGER,
                    optional_header_magic TEXT
                );
                CREATE TABLE IF NOT EXISTS authenticode (
                    observation_id INTEGER NOT NULL,
                    subject        TEXT,
                    issuer         TEXT,
                    serial_number  TEXT,
                    thumbprint     TEXT,
                    not_before     TEXT,
                    not_after      TEXT
                );
                CREATE TABLE IF NOT EXISTS msi_properties (
                    observation_id   INTEGER NOT NULL,
                    product_name     TEXT,
                    product_version  TEXT,
                    manufacturer     TEXT,
                    product_code     TEXT,
                    upgrade_code     TEXT,
                    product_language TEXT
                );
                CREATE TABLE IF NOT EXISTS appx_manifest (
                    observation_id         INTEGER NOT NULL,
                    name                   TEXT,
                    publisher              TEXT,
                    version                TEXT,
                    processor_architecture TEXT
                );
                """;
            create.ExecuteNonQuery();

            if (userVersion == 0)
            {
                using var stamp = _connection.CreateCommand();
                stamp.CommandText = $"PRAGMA user_version = {SchemaVersion};";
                stamp.ExecuteNonQuery();
            }
        }
        catch
        {
            _connection.Dispose();
            throw;
        }

        // Transaction unique : simplicité d'abord. Lots + reprise viendront quand une mesure le justifiera.
        _transaction = _connection.BeginTransaction();

        // La ligne du scan vit dans la même transaction : un scan interrompu ne laisse ni scan ni observations.
        using (var insertScan = _connection.CreateCommand())
        {
            insertScan.Transaction = _transaction;
            insertScan.CommandText = """
                INSERT INTO scans (volume_id, volume_label, root_path, started_at, extensions)
                VALUES ($volumeId, $volumeLabel, $rootPath, $startedAt, $extensions);
                SELECT last_insert_rowid();
                """;
            insertScan.Parameters.AddWithValue("$volumeId", scan.VolumeId);
            insertScan.Parameters.AddWithValue("$volumeLabel", (object?)scan.VolumeLabel ?? DBNull.Value);
            insertScan.Parameters.AddWithValue("$rootPath", scan.RootPath);
            insertScan.Parameters.AddWithValue("$startedAt", scan.StartedAt);
            insertScan.Parameters.AddWithValue("$extensions", (object?)scan.Extensions ?? DBNull.Value);
            _scanId = (long)insertScan.ExecuteScalar()!;
        }

        _insert = _connection.CreateCommand();
        _insert.Transaction = _transaction;
        _insert.CommandText = """
            INSERT INTO scan_observations (scan_id, path, size, sha256, scanned_at) VALUES ($scanId, $path, $size, $sha256, $scannedAt);
            SELECT last_insert_rowid();
            """;
        _insert.Parameters.AddWithValue("$scanId", _scanId); // fixé une fois, jamais réassigné dans Persist
        _pPath = _insert.Parameters.Add("$path", SqliteType.Text);
        _pSize = _insert.Parameters.Add("$size", SqliteType.Integer);
        _pSha256 = _insert.Parameters.Add("$sha256", SqliteType.Text);
        _pScannedAt = _insert.Parameters.Add("$scannedAt", SqliteType.Text);

        _insertVersion = _connection.CreateCommand();
        _insertVersion.Transaction = _transaction;
        _insertVersion.CommandText = """
            INSERT INTO version_info (observation_id, product_name, company_name, product_version, file_version)
            VALUES ($observationId, $productName, $companyName, $productVersion, $fileVersion);
            """;
        _pObservationId = _insertVersion.Parameters.Add("$observationId", SqliteType.Integer);
        _pProductName = _insertVersion.Parameters.Add("$productName", SqliteType.Text);
        _pCompanyName = _insertVersion.Parameters.Add("$companyName", SqliteType.Text);
        _pProductVersion = _insertVersion.Parameters.Add("$productVersion", SqliteType.Text);
        _pFileVersion = _insertVersion.Parameters.Add("$fileVersion", SqliteType.Text);

        _insertHeader = _connection.CreateCommand();
        _insertHeader.Transaction = _transaction;
        _insertHeader.CommandText = """
            INSERT INTO file_headers (observation_id, magic_hex, container)
            VALUES ($observationId, $magicHex, $container);
            """;
        _pHeaderObservationId = _insertHeader.Parameters.Add("$observationId", SqliteType.Integer);
        _pMagicHex = _insertHeader.Parameters.Add("$magicHex", SqliteType.Text);
        _pContainer = _insertHeader.Parameters.Add("$container", SqliteType.Text);

        _insertPe = _connection.CreateCommand();
        _insertPe.Transaction = _transaction;
        _insertPe.CommandText = """
            INSERT INTO pe_info (observation_id, machine, subsystem, characteristics, timestamp, optional_header_magic)
            VALUES ($observationId, $machine, $subsystem, $characteristics, $timestamp, $optionalHeaderMagic);
            """;
        _pPeObservationId = _insertPe.Parameters.Add("$observationId", SqliteType.Integer);
        _pMachine = _insertPe.Parameters.Add("$machine", SqliteType.Text);
        _pSubsystem = _insertPe.Parameters.Add("$subsystem", SqliteType.Text);
        _pCharacteristics = _insertPe.Parameters.Add("$characteristics", SqliteType.Integer);
        _pTimestamp = _insertPe.Parameters.Add("$timestamp", SqliteType.Integer);
        _pOptionalHeaderMagic = _insertPe.Parameters.Add("$optionalHeaderMagic", SqliteType.Text);

        _insertAuthenticode = _connection.CreateCommand();
        _insertAuthenticode.Transaction = _transaction;
        _insertAuthenticode.CommandText = """
            INSERT INTO authenticode (observation_id, subject, issuer, serial_number, thumbprint, not_before, not_after)
            VALUES ($observationId, $subject, $issuer, $serialNumber, $thumbprint, $notBefore, $notAfter);
            """;
        _pAuthObservationId = _insertAuthenticode.Parameters.Add("$observationId", SqliteType.Integer);
        _pSubject = _insertAuthenticode.Parameters.Add("$subject", SqliteType.Text);
        _pIssuer = _insertAuthenticode.Parameters.Add("$issuer", SqliteType.Text);
        _pSerialNumber = _insertAuthenticode.Parameters.Add("$serialNumber", SqliteType.Text);
        _pThumbprint = _insertAuthenticode.Parameters.Add("$thumbprint", SqliteType.Text);
        _pNotBefore = _insertAuthenticode.Parameters.Add("$notBefore", SqliteType.Text);
        _pNotAfter = _insertAuthenticode.Parameters.Add("$notAfter", SqliteType.Text);

        _insertMsi = _connection.CreateCommand();
        _insertMsi.Transaction = _transaction;
        _insertMsi.CommandText = """
            INSERT INTO msi_properties (observation_id, product_name, product_version, manufacturer, product_code, upgrade_code, product_language)
            VALUES ($observationId, $productName, $productVersion, $manufacturer, $productCode, $upgradeCode, $productLanguage);
            """;
        _pMsiObservationId = _insertMsi.Parameters.Add("$observationId", SqliteType.Integer);
        _pMsiProductName = _insertMsi.Parameters.Add("$productName", SqliteType.Text);
        _pMsiProductVersion = _insertMsi.Parameters.Add("$productVersion", SqliteType.Text);
        _pMsiManufacturer = _insertMsi.Parameters.Add("$manufacturer", SqliteType.Text);
        _pMsiProductCode = _insertMsi.Parameters.Add("$productCode", SqliteType.Text);
        _pMsiUpgradeCode = _insertMsi.Parameters.Add("$upgradeCode", SqliteType.Text);
        _pMsiProductLanguage = _insertMsi.Parameters.Add("$productLanguage", SqliteType.Text);

        _insertAppx = _connection.CreateCommand();
        _insertAppx.Transaction = _transaction;
        _insertAppx.CommandText = """
            INSERT INTO appx_manifest (observation_id, name, publisher, version, processor_architecture)
            VALUES ($observationId, $name, $publisher, $version, $processorArchitecture);
            """;
        _pAppxObservationId = _insertAppx.Parameters.Add("$observationId", SqliteType.Integer);
        _pAppxName = _insertAppx.Parameters.Add("$name", SqliteType.Text);
        _pAppxPublisher = _insertAppx.Parameters.Add("$publisher", SqliteType.Text);
        _pAppxVersion = _insertAppx.Parameters.Add("$version", SqliteType.Text);
        _pAppxProcessorArchitecture = _insertAppx.Parameters.Add("$processorArchitecture", SqliteType.Text);
    }

    /// <summary>Écrit l'observation dans les 7 tables et retourne l'observation_id attribué.</summary>
    public long Persist(FileObservation o)
    {
        _pPath.Value = o.Path;
        _pSize.Value = o.Size;
        _pSha256.Value = o.Sha256;
        _pScannedAt.Value = o.ScannedAt;
        var observationId = (long)_insert.ExecuteScalar()!;

        // Stocke ce que retourne l'API telle quelle. Aucune ressource VersionInfo → colonnes NULL, pas une erreur.
        _pObservationId.Value = observationId;
        _pProductName.Value = (object?)o.VersionInfo.ProductName ?? DBNull.Value;
        _pCompanyName.Value = (object?)o.VersionInfo.CompanyName ?? DBNull.Value;
        _pProductVersion.Value = (object?)o.VersionInfo.ProductVersion ?? DBNull.Value;
        _pFileVersion.Value = (object?)o.VersionInfo.FileVersion ?? DBNull.Value;
        _insertVersion.ExecuteNonQuery();

        _pHeaderObservationId.Value = observationId;
        _pMagicHex.Value = o.MagicHex;
        _pContainer.Value = (object?)o.Container ?? DBNull.Value;
        _insertHeader.ExecuteNonQuery();

        _pPeObservationId.Value = observationId;
        _pMachine.Value = (object?)o.PeInfo.Machine ?? DBNull.Value;
        _pSubsystem.Value = (object?)o.PeInfo.Subsystem ?? DBNull.Value;
        _pCharacteristics.Value = (object?)o.PeInfo.Characteristics ?? DBNull.Value;
        _pTimestamp.Value = (object?)o.PeInfo.Timestamp ?? DBNull.Value;
        _pOptionalHeaderMagic.Value = (object?)o.PeInfo.OptionalHeaderMagic ?? DBNull.Value;
        _insertPe.ExecuteNonQuery();

        _pAuthObservationId.Value = observationId;
        _pSubject.Value = (object?)o.Authenticode.Subject ?? DBNull.Value;
        _pIssuer.Value = (object?)o.Authenticode.Issuer ?? DBNull.Value;
        _pSerialNumber.Value = (object?)o.Authenticode.SerialNumber ?? DBNull.Value;
        _pThumbprint.Value = (object?)o.Authenticode.Thumbprint ?? DBNull.Value;
        _pNotBefore.Value = (object?)o.Authenticode.NotBefore ?? DBNull.Value;
        _pNotAfter.Value = (object?)o.Authenticode.NotAfter ?? DBNull.Value;
        _insertAuthenticode.ExecuteNonQuery();

        _pMsiObservationId.Value = observationId;
        _pMsiProductName.Value = (object?)o.MsiProperties.ProductName ?? DBNull.Value;
        _pMsiProductVersion.Value = (object?)o.MsiProperties.ProductVersion ?? DBNull.Value;
        _pMsiManufacturer.Value = (object?)o.MsiProperties.Manufacturer ?? DBNull.Value;
        _pMsiProductCode.Value = (object?)o.MsiProperties.ProductCode ?? DBNull.Value;
        _pMsiUpgradeCode.Value = (object?)o.MsiProperties.UpgradeCode ?? DBNull.Value;
        _pMsiProductLanguage.Value = (object?)o.MsiProperties.ProductLanguage ?? DBNull.Value;
        _insertMsi.ExecuteNonQuery();

        _pAppxObservationId.Value = observationId;
        _pAppxName.Value = (object?)o.AppxManifest.Name ?? DBNull.Value;
        _pAppxPublisher.Value = (object?)o.AppxManifest.Publisher ?? DBNull.Value;
        _pAppxVersion.Value = (object?)o.AppxManifest.Version ?? DBNull.Value;
        _pAppxProcessorArchitecture.Value = (object?)o.AppxManifest.ProcessorArchitecture ?? DBNull.Value;
        _insertAppx.ExecuteNonQuery();

        return observationId;
    }

    /// <summary>Projection JSON stricte des valeurs insérées par <see cref="Persist"/> — aucune valeur recalculée ni relue.</summary>
    public string ProjectJson(FileObservation o, long observationId)
    {
        return JsonSerializer.Serialize(new
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
            status = "ok", // "failed"/"partial" : réservés à l'option fichiers inaccessibles, non activée
        });
    }

    /// <summary>Valide la transaction unique du scan.</summary>
    public void Commit() => _transaction.Commit();

    public void Dispose()
    {
        _insert.Dispose();
        _insertVersion.Dispose();
        _insertHeader.Dispose();
        _insertPe.Dispose();
        _insertAuthenticode.Dispose();
        _insertMsi.Dispose();
        _insertAppx.Dispose();
        _transaction.Dispose();
        _connection.Dispose();
    }
}
