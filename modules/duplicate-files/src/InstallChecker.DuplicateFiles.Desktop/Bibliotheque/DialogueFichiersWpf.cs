using System.IO;
using Microsoft.Win32;

namespace InstallChecker.DuplicateFiles.Desktop.Bibliotheque;

public interface IDialogueFichiers
{
    string? ChoisirDossier(string? initial);
    string? OuvrirBase(string? initial);
    string? OuvrirJson(string? initial);
    string? SauvegarderSession(string? initial);
}

public sealed class DialogueFichiersWpf : IDialogueFichiers
{
    public string? ChoisirDossier(string? initial)
    {
        var dialogue = new OpenFolderDialog
        {
            Title = "Choisir un dossier à scanner",
            InitialDirectory = RepertoireInitial(initial),
            Multiselect = false
        };

        return dialogue.ShowDialog() == true ? dialogue.FolderName : null;
    }

    public string? OuvrirBase(string? initial) => OuvrirFichier(
        "Ouvrir une bibliothèque",
        "Base SQLite (*.db)|*.db|Tous les fichiers (*.*)|*.*",
        initial);

    public string? OuvrirJson(string? initial) => OuvrirFichier(
        "Importer un rapport de doublons",
        "Rapport JSON (*.json)|*.json|Tous les fichiers (*.*)|*.*",
        initial);

    public string? SauvegarderSession(string? initial)
    {
        var dialogue = new SaveFileDialog
        {
            Title = "Enregistrer la session",
            Filter = "Session JSON (*.json)|*.json",
            DefaultExt = ".json",
            AddExtension = true,
            InitialDirectory = RepertoireInitial(initial),
            FileName = NomInitial(initial, "bibliotheque.session.json")
        };

        return dialogue.ShowDialog() == true ? dialogue.FileName : null;
    }

    private static string? OuvrirFichier(string titre, string filtre, string? initial)
    {
        var dialogue = new OpenFileDialog
        {
            Title = titre,
            Filter = filtre,
            CheckFileExists = true,
            InitialDirectory = RepertoireInitial(initial),
            FileName = NomInitial(initial, string.Empty)
        };

        return dialogue.ShowDialog() == true ? dialogue.FileName : null;
    }

    private static string? RepertoireInitial(string? initial)
    {
        if (string.IsNullOrWhiteSpace(initial))
            return null;

        return Directory.Exists(initial) ? initial : Path.GetDirectoryName(initial);
    }

    private static string NomInitial(string? initial, string valeurParDefaut)
    {
        if (string.IsNullOrWhiteSpace(initial) || Directory.Exists(initial))
            return valeurParDefaut;

        return Path.GetFileName(initial);
    }
}
