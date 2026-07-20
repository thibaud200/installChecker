using System.Text.Json;
using InstallChecker.DuplicateFiles.Desktop.Presentation;

namespace InstallChecker.DuplicateFiles.Desktop.Tests;

public sealed class LecteurRapportDoublonsUiTests
{
    [Fact]
    public async Task Rapport_historique_reel_expose_sa_synthese_et_ses_groupes()
    {
        var chemin = Path.Combine(RacineDepot(), "rapport-doublons.json");
        var rapport = await new LecteurRapportDoublonsUi().LireFichierAsync(chemin, default);

        Assert.True(rapport.EstHistorique);
        Assert.Equal(6009, rapport.NombreGroupes);
        Assert.Equal(8579, rapport.NombreCandidats);
        Assert.NotEmpty(rapport.Groupes);
        Assert.StartsWith("legacy:domaine:", rapport.Groupes[0].GroupeId);
    }

    [Fact]
    public void Rapport_courant_projette_identifiants_roles_et_blocages()
    {
        using var document = JsonDocument.Parse("""
            {
              "VersionContrat": "duplicate-files/exact-duplicates/v1",
              "Synthese": {
                "NombreDeGroupes": 1,
                "NombreDeCandidatsASuppression": 1,
                "EspaceRecuperableOctets": 42
              },
              "Groupes": [{
                "GroupeId": "exact:sha256:abc",
                "TailleUnitaire": 42,
                "EspaceRecuperableOctets": 42,
                "Confiance": "Certaine",
                "ContenuSha256": "abc",
                "Exemplaires": [{
                  "FichierId": "file:1",
                  "Rang": 2,
                  "Role": "Candidat",
                  "Fichier": {
                    "ActeId": 12,
                    "Chemin": "C:\\a.exe",
                    "Taille": 42,
                    "VolumeLabel": "Archives"
                  },
                  "Actions": [{
                    "Action": "AjouterAuPlanDeSuppression",
                    "Autorisee": false,
                    "Blocages": ["CheminProtege"]
                  }]
                }]
              }]
            }
            """);

        var rapport = new LecteurRapportDoublonsUi().Lire(document.RootElement);
        var groupe = Assert.Single(rapport.Groupes);
        var fichier = Assert.Single(groupe.Fichiers);

        Assert.False(rapport.EstHistorique);
        Assert.Equal("exact:sha256:abc", groupe.GroupeId);
        Assert.Equal("file:1", fichier.FichierId);
        Assert.Equal("Candidat", fichier.Role);
        Assert.Equal("Archives", fichier.Volume);
        Assert.Contains("CheminProtege", fichier.Blocages);
    }

    private static string RacineDepot()
    {
        var courant = new DirectoryInfo(AppContext.BaseDirectory);
        while (courant is not null)
        {
            if (File.Exists(Path.Combine(courant.FullName, "InstallChecker.slnx")))
                return courant.FullName;

            courant = courant.Parent;
        }

        throw new DirectoryNotFoundException("Racine du dépôt introuvable.");
    }
}
