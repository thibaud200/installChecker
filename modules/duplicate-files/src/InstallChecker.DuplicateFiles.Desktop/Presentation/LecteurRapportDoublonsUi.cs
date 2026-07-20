using System.IO;
using System.Text.Json;

namespace InstallChecker.DuplicateFiles.Desktop.Presentation;

public sealed class LecteurRapportDoublonsUi
{
    private const string ContratCourant = "duplicate-files/exact-duplicates/v1";

    public RapportDoublonsUi Lire(JsonElement racine)
    {
        if (!racine.TryGetProperty("VersionContrat", out var version))
            return LireHistorique(racine);

        if (version.GetString() != ContratCourant)
            throw new InvalidDataException($"Contrat de doublons non pris en charge : {version.GetString()}.");

        return LireCourant(racine);
    }

    public async Task<RapportDoublonsUi> LireFichierAsync(
        string chemin,
        CancellationToken cancellationToken)
    {
        var json = await ChargerJsonAsync(chemin, cancellationToken);
        return Lire(json);
    }

    public async Task<JsonElement> ChargerJsonAsync(
        string chemin,
        CancellationToken cancellationToken)
    {
        await using var flux = new FileStream(
            chemin,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            65_536,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var lecteur = new StreamReader(flux, detectEncodingFromByteOrderMarks: true);
        var json = await lecteur.ReadToEndAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static RapportDoublonsUi LireCourant(JsonElement racine)
    {
        var groupesJson = racine.GetProperty("Groupes");
        var groupes = new List<GroupeDoublonUi>(groupesJson.GetArrayLength());
        foreach (var groupeJson in groupesJson.EnumerateArray())
        {
            var fichiers = groupeJson.GetProperty("Exemplaires")
                .EnumerateArray()
                .Select(LireFichierCourant)
                .ToArray();
            groupes.Add(new GroupeDoublonUi(
                groupeJson.GetProperty("GroupeId").GetString()!,
                LireInt64(groupeJson, "TailleUnitaire"),
                LireInt64(groupeJson, "EspaceRecuperableOctets"),
                LireTexte(groupeJson, "Confiance") ?? "Certaine",
                LireTexte(groupeJson, "ContenuSha256"),
                fichiers));
        }

        var synthese = racine.GetProperty("Synthese");
        return new RapportDoublonsUi(
            false,
            LireInt32(synthese, "NombreDeGroupes", groupes.Count),
            LireInt32(synthese, "NombreDeCandidatsASuppression",
                groupes.Sum(g => g.Fichiers.Count(f => f.Role == "Candidat"))),
            LireInt64(synthese, "EspaceRecuperableOctets"),
            groupes);
    }

    private static RapportDoublonsUi LireHistorique(JsonElement racine)
    {
        var groupesJson = racine.GetProperty("Groupes");
        var groupes = new List<GroupeDoublonUi>(groupesJson.GetArrayLength());
        foreach (var groupeJson in groupesJson.EnumerateArray())
        {
            var actes = groupeJson.GetProperty("Domaine")
                .EnumerateArray()
                .Select(id => id.GetInt64())
                .Order()
                .ToArray();
            var fichiers = groupeJson.GetProperty("Exemplaires")
                .EnumerateArray()
                .Select(LireFichierHistorique)
                .ToArray();
            groupes.Add(new GroupeDoublonUi(
                $"legacy:domaine:{string.Join('-', actes)}",
                LireInt64(groupeJson, "TailleUnitaire"),
                LireInt64(groupeJson, "EspaceRecuperableOctets"),
                "Certaine",
                null,
                fichiers));
        }

        var synthese = racine.GetProperty("Synthese");
        return new RapportDoublonsUi(
            true,
            LireInt32(synthese, "NombreDeGroupes", groupes.Count),
            LireInt32(synthese, "NombreDeCandidatsASuppression",
                groupes.Sum(g => Math.Max(0, g.Fichiers.Count - 1))),
            LireInt64(synthese, "EspaceRecuperableOctets"),
            groupes);
    }

    private static FichierDoublonUi LireFichierCourant(JsonElement exemplaire)
    {
        var fichier = exemplaire.GetProperty("Fichier");
        var blocages = exemplaire.TryGetProperty("Actions", out var actions)
            ? actions.EnumerateArray()
                .Where(a => a.TryGetProperty("Blocages", out _))
                .SelectMany(a => a.GetProperty("Blocages").EnumerateArray())
                .Select(b => b.GetString()!)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
            : [];
        var volume = LireTexte(fichier, "VolumeLabel") ?? LireTexte(fichier, "VolumeId");

        return new FichierDoublonUi(
            exemplaire.GetProperty("FichierId").GetString()!,
            LireInt64(fichier, "ActeId"),
            fichier.GetProperty("Chemin").GetString()!,
            LireInt64(fichier, "Taille"),
            LireInt32(exemplaire, "Rang"),
            LireTexte(exemplaire, "Role") ?? "",
            volume,
            blocages);
    }

    private static FichierDoublonUi LireFichierHistorique(JsonElement exemplaire)
    {
        var fichier = exemplaire.GetProperty("Fichier");
        var acteId = LireInt64(fichier, "ActeId");
        return new FichierDoublonUi(
            $"legacy:acte:{acteId}",
            acteId,
            fichier.GetProperty("Chemin").GetString()!,
            LireInt64(fichier, "Taille"),
            LireInt32(exemplaire, "Rang"),
            LireTexte(exemplaire, "Etiquette") ?? "",
            null,
            []);
    }

    private static string? LireTexte(JsonElement element, string propriete) =>
        element.TryGetProperty(propriete, out var valeur) && valeur.ValueKind == JsonValueKind.String
            ? valeur.GetString()
            : null;

    private static int LireInt32(JsonElement element, string propriete, int valeurParDefaut = 0) =>
        element.TryGetProperty(propriete, out var valeur) && valeur.TryGetInt32(out var nombre)
            ? nombre
            : valeurParDefaut;

    private static long LireInt64(JsonElement element, string propriete) =>
        element.TryGetProperty(propriete, out var valeur) && valeur.TryGetInt64(out var nombre)
            ? nombre
            : 0;
}
