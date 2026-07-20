using System.Text.Json;

namespace InstallChecker.Tests;

/// <summary>
/// La commande <c>duplicates</c> — v1 du module Duplicate Files (plan rev3, D1/D2) : un état Ω
/// désigné, strate contenu exclusivement. Bout-en-bout sur l'artefact et le registre réels, même
/// régime d'erreur que <c>identity</c> puisqu'elle délègue au même <c>Porteur.Deriver</c>
/// (voir <see cref="IdentityCommandTests"/> pour la batterie complète des erreurs contractuelles,
/// déjà prouvée sur cet appel — non redupliquée ici).
/// </summary>
public class DuplicatesCommandTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "duplicates-cli-tests-" + Guid.NewGuid())).FullName;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private static string RacineDuDepot()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        return repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable");
    }

    private static string CheminOracle() => Path.Combine(RacineDuDepot(), "tests", "oracle", "corpus1-postA1.db");

    private static string CheminRegistreReel() => Path.Combine(RacineDuDepot(), "registre");

    private static IReadOnlyList<string> Textes(JsonElement tableau) =>
        tableau.EnumerateArray().Select(e => e.GetString()!).ToList();

    [Fact]
    public void Le_corpus_reel_produit_112_groupes_une_synthese_et_une_note_de_capacite()
    {
        var sortie = new StringWriter();
        var erreurs = new StringWriter();

        var code = DuplicatesCommand.Deriver(CheminOracle(), CheminRegistreReel(), sortie, erreurs);

        Assert.Equal(0, code);
        Assert.Empty(erreurs.ToString());

        using var json = JsonDocument.Parse(sortie.ToString());
        var racine = json.RootElement;

        // 112 groupes (108 paires + 4 triplets).
        Assert.Equal(112, racine.GetProperty("Groupes").GetArrayLength());
        Assert.Equal("duplicate-files/exact-duplicates/v1", racine.GetProperty("VersionContrat").GetString());

        var premierGroupe = racine.GetProperty("Groupes")[0];
        Assert.Matches("^exact:sha256:[0-9a-f]{64}$", premierGroupe.GetProperty("GroupeId").GetString());
        Assert.Equal("ExactDuplicate", premierGroupe.GetProperty("Categorie").GetString());
        Assert.Equal("Certaine", premierGroupe.GetProperty("Confiance").GetString());
        var preuve = Assert.Single(premierGroupe.GetProperty("Preuves").EnumerateArray());
        Assert.Equal("Sha256Identique", preuve.GetProperty("Type").GetString());

        var exemplaires = premierGroupe.GetProperty("Exemplaires").EnumerateArray().ToList();
        var recommande = exemplaires.Single(e => e.GetProperty("Role").GetString() == "RecommandeAConserver");
        Assert.Equal(recommande.GetProperty("FichierId").GetString(), premierGroupe.GetProperty("FichierRecommandeId").GetString());
        Assert.Contains(
            recommande.GetProperty("Actions").EnumerateArray(),
            a => a.GetProperty("Action").GetString() == "AjouterAuPlanDeSuppression"
              && !a.GetProperty("Autorisee").GetBoolean());

        // Synthèse présente, avec ses cinq agrégats, espace récupérable strictement positif.
        var synthese = racine.GetProperty("Synthese");
        Assert.Equal(112, synthese.GetProperty("NombreDeGroupes").GetInt32());
        Assert.Equal(JsonValueKind.Number, synthese.GetProperty("NombreDeFichiersRedondants").ValueKind);
        Assert.True(synthese.GetProperty("EspaceRecuperableOctets").GetInt64() > 0);
        Assert.Equal(JsonValueKind.Number, synthese.GetProperty("NombreDeFichiersAConserver").ValueKind);
        Assert.Equal(JsonValueKind.Number, synthese.GetProperty("NombreDeCandidatsASuppression").ValueKind);

        // Note de capacité : les quatre strates supérieures indisponibles.
        var note = racine.GetProperty("Note");
        Assert.NotEqual(JsonValueKind.Null, note.ValueKind);
        var strates = Textes(note.GetProperty("StratesIndisponibles"));
        Assert.Contains("variante", strates);
        Assert.Contains("version", strates);
        Assert.Contains("identité", strates);
        Assert.Contains("famille", strates);

        // Aucune fuite de type moteur : plus de champ NonTranches.
        Assert.False(racine.TryGetProperty("NonTranches", out _));
    }

    [Fact]
    public void Deux_emissions_du_rapport_sont_identiques()
    {
        var premiere = new StringWriter();
        var seconde = new StringWriter();

        Assert.Equal(0, DuplicatesCommand.Deriver(CheminOracle(), CheminRegistreReel(), premiere, new StringWriter()));
        Assert.Equal(0, DuplicatesCommand.Deriver(CheminOracle(), CheminRegistreReel(), seconde, new StringWriter()));

        Assert.Equal(premiere.ToString(), seconde.ToString());
    }

    [Fact]
    public void Une_base_absente_produit_lerreur_du_moteur_telle_quelle()
    {
        var erreurs = new StringWriter();

        var code = DuplicatesCommand.Deriver(
            Path.Combine(_root, "absente.db"), CheminRegistreReel(), new StringWriter(), erreurs);

        Assert.Equal(1, code);
        Assert.False(string.IsNullOrWhiteSpace(erreurs.ToString()));
    }
}
