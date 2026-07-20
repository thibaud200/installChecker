using System.IO;
using System.Text.Json;

namespace InstallChecker.DuplicateFiles.Desktop.Presentation;

public sealed class LecteurRapportVersionsUi
{
    private const string ContratCourant = "duplicate-files/version-redundancy/v1";

    public RapportVersionsUi Lire(JsonElement racine)
    {
        var version = racine.TryGetProperty("VersionContrat", out var valeur)
            ? valeur.GetString()
            : null;
        if (version != ContratCourant)
            throw new InvalidDataException($"Contrat de versions non pris en charge : {version ?? "absent"}.");

        var groupesJson = racine.GetProperty("Groupes");
        var groupes = groupesJson.EnumerateArray().Select(LireGroupe).ToArray();
        var synthese = racine.GetProperty("Synthese");
        return new RapportVersionsUi(
            LireInt32(synthese, "NombreGroupes", groupes.Length),
            LireInt32(synthese, "NombreVersionsAnterieures"),
            groupes);
    }

    private static GroupeVersionUi LireGroupe(JsonElement groupe)
    {
        var variante = groupe.GetProperty("Variante");
        var artefacts = groupe.GetProperty("Artefacts")
            .EnumerateArray()
            .Select(LireArtefact)
            .ToArray();
        return new GroupeVersionUi(
            groupe.GetProperty("GroupeId").GetString()!,
            groupe.GetProperty("Famille").GetString()!,
            groupe.GetProperty("VersionReference").GetString()!,
            groupe.GetProperty("Confiance").GetString()!,
            groupe.GetProperty("Variante").GetProperty("Format").GetString()!,
            LireTexte(variante, "Architecture"),
            LireTexte(variante, "Langue"),
            variante.TryGetProperty("Partielle", out var partielle) && partielle.GetBoolean(),
            LireChaines(groupe, "Blocages"),
            artefacts);
    }

    private static ArtefactVersionUi LireArtefact(JsonElement artefact)
    {
        var chemins = artefact.GetProperty("Fichiers")
            .EnumerateArray()
            .Select(f => f.GetProperty("Chemin").GetString()!)
            .ToArray();
        return new ArtefactVersionUi(
            artefact.GetProperty("ContenuSha256").GetString()!,
            artefact.GetProperty("Version").GetString()!,
            LireTexte(artefact, "Role"),
            chemins,
            LireChaines(artefact, "Blocages"));
    }

    private static IReadOnlyList<string> LireChaines(JsonElement element, string propriete) =>
        element.TryGetProperty(propriete, out var valeurs)
            ? valeurs.EnumerateArray().Select(v => v.GetString()!).ToArray()
            : [];

    private static string? LireTexte(JsonElement element, string propriete) =>
        element.TryGetProperty(propriete, out var valeur) && valeur.ValueKind == JsonValueKind.String
            ? valeur.GetString()
            : null;

    private static int LireInt32(JsonElement element, string propriete, int valeurParDefaut = 0) =>
        element.TryGetProperty(propriete, out var valeur) && valeur.TryGetInt32(out var nombre)
            ? nombre
            : valeurParDefaut;
}
