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
        insert.CommandText =
            "INSERT INTO scan_observations (path, size, sha256, scanned_at) VALUES ($path, $size, $sha256, $scannedAt);";
        var pPath = insert.Parameters.Add("$path", SqliteType.Text);
        var pSize = insert.Parameters.Add("$size", SqliteType.Integer);
        var pSha256 = insert.Parameters.Add("$sha256", SqliteType.Text);
        var pScannedAt = insert.Parameters.Add("$scannedAt", SqliteType.Text);

        long fileCount = 0, errorCount = 0;
        foreach (var file in new DirectoryInfo(root).EnumerateFiles("*", options))
        {
            try
            {
                using var stream = file.OpenRead();
                var sha256 = Convert.ToHexStringLower(SHA256.HashData(stream)); // lecture en flux, jamais le fichier entier en mémoire

                pPath.Value = file.FullName;
                pSize.Value = file.Length;
                pSha256.Value = sha256;
                pScannedAt.Value = DateTime.UtcNow.ToString("O");
                insert.ExecuteNonQuery();

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
