using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace InstallChecker;

public static class ScanCommand
{
    /// <summary>
    /// Parcourt récursivement <paramref name="root"/>, écrit "chemin complet TAB taille TAB sha256"
    /// par fichier et enregistre chaque ligne comme observation dans la base SQLite <paramref name="dbPath"/>.
    /// La table est append-only : chaque exécution ajoute de nouvelles observations, sans dédoublonnage.
    /// </summary>
    /// <returns>0 si le scan s'est terminé (même avec erreurs locales), 1 si la racine ou la base est invalide.</returns>
    public static int Run(string root, string dbPath, TextWriter output, TextWriter errors)
    {
        if (!Directory.Exists(root))
        {
            errors.WriteLine($"Erreur : dossier introuvable : {root}");
            return 1;
        }

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        try
        {
            connection.Open(); // crée le fichier si absent
            using var create = connection.CreateCommand();
            create.CommandText = """
                CREATE TABLE IF NOT EXISTS scan_observations (
                    id         INTEGER PRIMARY KEY AUTOINCREMENT,
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
        }
        catch (SqliteException ex)
        {
            errors.WriteLine($"Erreur : base inaccessible : {dbPath} : {ex.Message}");
            return 1;
        }

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,               // dossier/fichier illisible : ignoré, jamais d'interruption
            AttributesToSkip = FileAttributes.None,  // inclut fichiers cachés et système
        };

        // Transaction unique : simplicité d'abord. Lots + reprise viendront quand une mesure le justifiera.
        using var transaction = connection.BeginTransaction();
        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = """
            INSERT INTO scan_observations (path, size, sha256, scanned_at) VALUES ($path, $size, $sha256, $scannedAt);
            SELECT last_insert_rowid();
            """;
        var pPath = insert.Parameters.Add("$path", SqliteType.Text);
        var pSize = insert.Parameters.Add("$size", SqliteType.Integer);
        var pSha256 = insert.Parameters.Add("$sha256", SqliteType.Text);
        var pScannedAt = insert.Parameters.Add("$scannedAt", SqliteType.Text);

        using var insertVersion = connection.CreateCommand();
        insertVersion.Transaction = transaction;
        insertVersion.CommandText = """
            INSERT INTO version_info (observation_id, product_name, company_name, product_version, file_version)
            VALUES ($observationId, $productName, $companyName, $productVersion, $fileVersion);
            """;
        var pObservationId = insertVersion.Parameters.Add("$observationId", SqliteType.Integer);
        var pProductName = insertVersion.Parameters.Add("$productName", SqliteType.Text);
        var pCompanyName = insertVersion.Parameters.Add("$companyName", SqliteType.Text);
        var pProductVersion = insertVersion.Parameters.Add("$productVersion", SqliteType.Text);
        var pFileVersion = insertVersion.Parameters.Add("$fileVersion", SqliteType.Text);

        using var insertHeader = connection.CreateCommand();
        insertHeader.Transaction = transaction;
        insertHeader.CommandText = """
            INSERT INTO file_headers (observation_id, magic_hex, container)
            VALUES ($observationId, $magicHex, $container);
            """;
        var pHeaderObservationId = insertHeader.Parameters.Add("$observationId", SqliteType.Integer);
        var pMagicHex = insertHeader.Parameters.Add("$magicHex", SqliteType.Text);
        var pContainer = insertHeader.Parameters.Add("$container", SqliteType.Text);

        using var insertPe = connection.CreateCommand();
        insertPe.Transaction = transaction;
        insertPe.CommandText = """
            INSERT INTO pe_info (observation_id, machine, subsystem, characteristics, timestamp, optional_header_magic)
            VALUES ($observationId, $machine, $subsystem, $characteristics, $timestamp, $optionalHeaderMagic);
            """;
        var pPeObservationId = insertPe.Parameters.Add("$observationId", SqliteType.Integer);
        var pMachine = insertPe.Parameters.Add("$machine", SqliteType.Text);
        var pSubsystem = insertPe.Parameters.Add("$subsystem", SqliteType.Text);
        var pCharacteristics = insertPe.Parameters.Add("$characteristics", SqliteType.Integer);
        var pTimestamp = insertPe.Parameters.Add("$timestamp", SqliteType.Integer);
        var pOptionalHeaderMagic = insertPe.Parameters.Add("$optionalHeaderMagic", SqliteType.Text);

        using var insertAuthenticode = connection.CreateCommand();
        insertAuthenticode.Transaction = transaction;
        insertAuthenticode.CommandText = """
            INSERT INTO authenticode (observation_id, subject, issuer, serial_number, thumbprint, not_before, not_after)
            VALUES ($observationId, $subject, $issuer, $serialNumber, $thumbprint, $notBefore, $notAfter);
            """;
        var pAuthObservationId = insertAuthenticode.Parameters.Add("$observationId", SqliteType.Integer);
        var pSubject = insertAuthenticode.Parameters.Add("$subject", SqliteType.Text);
        var pIssuer = insertAuthenticode.Parameters.Add("$issuer", SqliteType.Text);
        var pSerialNumber = insertAuthenticode.Parameters.Add("$serialNumber", SqliteType.Text);
        var pThumbprint = insertAuthenticode.Parameters.Add("$thumbprint", SqliteType.Text);
        var pNotBefore = insertAuthenticode.Parameters.Add("$notBefore", SqliteType.Text);
        var pNotAfter = insertAuthenticode.Parameters.Add("$notAfter", SqliteType.Text);

        using var insertMsi = connection.CreateCommand();
        insertMsi.Transaction = transaction;
        insertMsi.CommandText = """
            INSERT INTO msi_properties (observation_id, product_name, product_version, manufacturer, product_code, upgrade_code, product_language)
            VALUES ($observationId, $productName, $productVersion, $manufacturer, $productCode, $upgradeCode, $productLanguage);
            """;
        var pMsiObservationId = insertMsi.Parameters.Add("$observationId", SqliteType.Integer);
        var pMsiProductName = insertMsi.Parameters.Add("$productName", SqliteType.Text);
        var pMsiProductVersion = insertMsi.Parameters.Add("$productVersion", SqliteType.Text);
        var pMsiManufacturer = insertMsi.Parameters.Add("$manufacturer", SqliteType.Text);
        var pMsiProductCode = insertMsi.Parameters.Add("$productCode", SqliteType.Text);
        var pMsiUpgradeCode = insertMsi.Parameters.Add("$upgradeCode", SqliteType.Text);
        var pMsiProductLanguage = insertMsi.Parameters.Add("$productLanguage", SqliteType.Text);

        using var insertAppx = connection.CreateCommand();
        insertAppx.Transaction = transaction;
        insertAppx.CommandText = """
            INSERT INTO appx_manifest (observation_id, name, publisher, version, processor_architecture)
            VALUES ($observationId, $name, $publisher, $version, $processorArchitecture);
            """;
        var pAppxObservationId = insertAppx.Parameters.Add("$observationId", SqliteType.Integer);
        var pAppxName = insertAppx.Parameters.Add("$name", SqliteType.Text);
        var pAppxPublisher = insertAppx.Parameters.Add("$publisher", SqliteType.Text);
        var pAppxVersion = insertAppx.Parameters.Add("$version", SqliteType.Text);
        var pAppxProcessorArchitecture = insertAppx.Parameters.Add("$processorArchitecture", SqliteType.Text);

        long fileCount = 0, errorCount = 0;
        foreach (var file in new DirectoryInfo(root).EnumerateFiles("*", options))
        {
            try
            {
                // Toutes les lectures d'abord (chaque capacité ouvre le fichier indépendamment),
                // toutes les écritures ensuite : aucune observation partielle en cas d'échec de lecture.
                string sha256;
                using (var stream = file.OpenRead())
                    sha256 = Convert.ToHexStringLower(SHA256.HashData(stream)); // lecture en flux, jamais le fichier entier en mémoire
                var versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);
                var (magicHex, container) = FileHeaderExtractor.Read(file.FullName);
                var peInfo = PeInfoExtractor.Read(file.FullName);
                var authenticode = AuthenticodeExtractor.Read(file.FullName);
                var msiProperties = MsiPropertiesExtractor.Read(file.FullName);
                var appxManifest = AppxManifestExtractor.Read(file.FullName);

                pPath.Value = file.FullName;
                pSize.Value = file.Length;
                pSha256.Value = sha256;
                pScannedAt.Value = DateTime.UtcNow.ToString("O");
                var observationId = (long)insert.ExecuteScalar()!;

                // Stocke ce que retourne l'API telle quelle. Aucune ressource VersionInfo → colonnes NULL, pas une erreur.
                pObservationId.Value = observationId;
                pProductName.Value = (object?)versionInfo.ProductName ?? DBNull.Value;
                pCompanyName.Value = (object?)versionInfo.CompanyName ?? DBNull.Value;
                pProductVersion.Value = (object?)versionInfo.ProductVersion ?? DBNull.Value;
                pFileVersion.Value = (object?)versionInfo.FileVersion ?? DBNull.Value;
                insertVersion.ExecuteNonQuery();

                pHeaderObservationId.Value = observationId;
                pMagicHex.Value = magicHex;
                pContainer.Value = (object?)container ?? DBNull.Value;
                insertHeader.ExecuteNonQuery();

                pPeObservationId.Value = observationId;
                pMachine.Value = (object?)peInfo.Machine ?? DBNull.Value;
                pSubsystem.Value = (object?)peInfo.Subsystem ?? DBNull.Value;
                pCharacteristics.Value = (object?)peInfo.Characteristics ?? DBNull.Value;
                pTimestamp.Value = (object?)peInfo.Timestamp ?? DBNull.Value;
                pOptionalHeaderMagic.Value = (object?)peInfo.OptionalHeaderMagic ?? DBNull.Value;
                insertPe.ExecuteNonQuery();

                pAuthObservationId.Value = observationId;
                pSubject.Value = (object?)authenticode.Subject ?? DBNull.Value;
                pIssuer.Value = (object?)authenticode.Issuer ?? DBNull.Value;
                pSerialNumber.Value = (object?)authenticode.SerialNumber ?? DBNull.Value;
                pThumbprint.Value = (object?)authenticode.Thumbprint ?? DBNull.Value;
                pNotBefore.Value = (object?)authenticode.NotBefore ?? DBNull.Value;
                pNotAfter.Value = (object?)authenticode.NotAfter ?? DBNull.Value;
                insertAuthenticode.ExecuteNonQuery();

                pMsiObservationId.Value = observationId;
                pMsiProductName.Value = (object?)msiProperties.ProductName ?? DBNull.Value;
                pMsiProductVersion.Value = (object?)msiProperties.ProductVersion ?? DBNull.Value;
                pMsiManufacturer.Value = (object?)msiProperties.Manufacturer ?? DBNull.Value;
                pMsiProductCode.Value = (object?)msiProperties.ProductCode ?? DBNull.Value;
                pMsiUpgradeCode.Value = (object?)msiProperties.UpgradeCode ?? DBNull.Value;
                pMsiProductLanguage.Value = (object?)msiProperties.ProductLanguage ?? DBNull.Value;
                insertMsi.ExecuteNonQuery();

                pAppxObservationId.Value = observationId;
                pAppxName.Value = (object?)appxManifest.Name ?? DBNull.Value;
                pAppxPublisher.Value = (object?)appxManifest.Publisher ?? DBNull.Value;
                pAppxVersion.Value = (object?)appxManifest.Version ?? DBNull.Value;
                pAppxProcessorArchitecture.Value = (object?)appxManifest.ProcessorArchitecture ?? DBNull.Value;
                insertAppx.ExecuteNonQuery();

                output.WriteLine($"{file.FullName}\t{file.Length}\t{sha256}");
                fileCount++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                errors.WriteLine($"Erreur : {file.FullName} : {ex.Message}");
                errorCount++;
            }
        }

        transaction.Commit();

        // Résumé sur stderr : stdout reste un flux de données pur.
        errors.WriteLine($"Scan terminé : {fileCount} fichier(s), {errorCount} erreur(s) locale(s). Base : {dbPath}");
        return 0;
    }
}
