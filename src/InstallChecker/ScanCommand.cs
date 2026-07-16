using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace InstallChecker;

public static class ScanCommand
{
    /// <summary>
    /// Parcourt récursivement <paramref name="root"/>, écrit une ligne par fichier sur stdout
    /// (TSV "chemin TAB taille TAB sha256" par défaut, JSON Lines si <paramref name="jsonOutput"/>)
    /// et enregistre chaque observation dans la base SQLite <paramref name="dbPath"/> via <see cref="ObservationStore"/>.
    /// Si <paramref name="extensions"/> est non vide, seuls les fichiers dont l'extension y figure sont
    /// observés (comparaison insensible à la casse ; le point de tête est optionnel, « exe » = « .exe »).
    /// </summary>
    /// <returns>0 si le scan s'est terminé (même avec erreurs locales), 1 si la racine ou la base est invalide.</returns>
    public static int Run(string root, string dbPath, bool jsonOutput, TextWriter output, TextWriter errors, IReadOnlyCollection<string>? extensions = null)
    {
        if (!Directory.Exists(root))
        {
            errors.WriteLine($"Erreur : dossier introuvable : {root}");
            return 1;
        }

        // Identité du volume résolue avant toute écriture : un volume irrésoluble corromprait le
        // remplacement de l'état courant (spec multi-disque D3) — on refuse de démarrer.
        VolumeIdentity volume;
        try
        {
            volume = VolumeIdentityExtractor.Resolve(root);
        }
        catch (InvalidOperationException ex)
        {
            errors.WriteLine(ex.Message);
            return 1;
        }

        var declaration = new ScanDeclaration(
            volume.VolumeId,
            volume.VolumeLabel,
            Path.GetFullPath(root),
            DateTime.UtcNow.ToString("O"),
            extensions is { Count: > 0 } ? string.Join(",", extensions) : null);

        ObservationStore store;
        try
        {
            store = new ObservationStore(dbPath, declaration);
        }
        catch (SqliteException ex)
        {
            errors.WriteLine($"Erreur : base inaccessible : {dbPath} : {ex.Message}");
            return 1;
        }
        catch (InvalidDataException ex)
        {
            errors.WriteLine(ex.Message); // user_version inattendu : le message du store est déjà complet
            return 1;
        }

        // Filtre d'extensions optionnel : normalisé une fois (point de tête optionnel, insensible à la casse).
        var filtreExtensions = extensions is { Count: > 0 }
            ? extensions.Select(e => e.StartsWith('.') ? e : "." + e).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : null;

        using (store)
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,               // dossier/fichier illisible : ignoré, jamais d'interruption
                AttributesToSkip = FileAttributes.None,  // inclut fichiers cachés et système
            };

            long fileCount = 0, errorCount = 0;
            foreach (var file in new DirectoryInfo(root).EnumerateFiles("*", options))
            {
                if (filtreExtensions is not null && !filtreExtensions.Contains(file.Extension))
                    continue;

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

                    var observation = new FileObservation(
                        Path: file.FullName,
                        Size: file.Length,
                        Sha256: sha256,
                        ScannedAt: DateTime.UtcNow.ToString("O"),
                        MagicHex: magicHex,
                        Container: container,
                        VersionInfo: versionInfo,
                        PeInfo: peInfo,
                        Authenticode: authenticode,
                        MsiProperties: msiProperties,
                        AppxManifest: appxManifest);

                    var observationId = store.Persist(observation);

                    if (jsonOutput)
                        output.WriteLine(store.ProjectJson(observation, observationId));
                    else
                        output.WriteLine($"{file.FullName}\t{file.Length}\t{sha256}");
                    fileCount++;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    errors.WriteLine($"Erreur : {file.FullName} : {ex.Message}");
                    errorCount++;
                }
            }

            store.Commit();

            // Résumé sur stderr : stdout reste un flux de données pur.
            errors.WriteLine($"Scan terminé : {fileCount} fichier(s), {errorCount} erreur(s) locale(s). Base : {dbPath}");
            return 0;
        }
    }
}
