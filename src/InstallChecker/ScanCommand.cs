namespace InstallChecker;

public static class ScanCommand
{
    /// <summary>Parcourt récursivement <paramref name="root"/> et écrit "chemin complet TAB taille" par fichier.</summary>
    /// <returns>0 si le scan s'est terminé (même avec erreurs locales), 1 si le dossier racine est invalide.</returns>
    public static int Run(string root, TextWriter output, TextWriter errors)
    {
        if (!Directory.Exists(root))
        {
            errors.WriteLine($"Erreur : dossier introuvable : {root}");
            return 1;
        }

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,               // dossier/fichier illisible : ignoré, jamais d'interruption
            AttributesToSkip = FileAttributes.None,  // inclut fichiers cachés et système
        };

        long fileCount = 0, errorCount = 0;
        foreach (var file in new DirectoryInfo(root).EnumerateFiles("*", options))
        {
            try
            {
                output.WriteLine($"{file.FullName}\t{file.Length}");
                fileCount++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                errors.WriteLine($"Erreur : {file.FullName} : {ex.Message}");
                errorCount++;
            }
        }

        // Résumé sur stderr : stdout reste un flux de données pur.
        errors.WriteLine($"Scan terminé : {fileCount} fichier(s), {errorCount} erreur(s) locale(s).");
        return 0;
    }
}
