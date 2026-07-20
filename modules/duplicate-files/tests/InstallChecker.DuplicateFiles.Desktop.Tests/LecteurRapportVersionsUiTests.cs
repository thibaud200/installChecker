using System.Text.Json;
using InstallChecker.DuplicateFiles.Desktop.Presentation;

namespace InstallChecker.DuplicateFiles.Desktop.Tests;

public sealed class LecteurRapportVersionsUiTests
{
    [Fact]
    public void Rapport_versionne_projette_variante_confiance_et_blocages()
    {
        using var document = JsonDocument.Parse("""
            {
              "VersionContrat": "duplicate-files/version-redundancy/v1",
              "Synthese": {
                "NombreGroupes": 1,
                "NombreVersionsAnterieures": 1
              },
              "Groupes": [{
                "GroupeId": "version:outil:x64",
                "Famille": "Outil",
                "VersionReference": "2.0.0",
                "Confiance": "Forte",
                "Variante": {
                  "Format": "exe",
                  "Architecture": "x64",
                  "Langue": null,
                  "Partielle": true
                },
                "Blocages": ["RevueHumaineObligatoire"],
                "Artefacts": [{
                  "ContenuSha256": "abc",
                  "Version": "1.0.0",
                  "Role": "VersionAnterieure",
                  "Fichiers": [{ "Chemin": "C:\\outil-1.exe" }],
                  "Blocages": ["SuppressionAutomatiqueInterdite"]
                }]
              }]
            }
            """);

        var rapport = new LecteurRapportVersionsUi().Lire(document.RootElement);
        var groupe = Assert.Single(rapport.Groupes);

        Assert.Equal("Outil", groupe.Famille);
        Assert.Equal("Forte", groupe.Confiance);
        Assert.Equal("x64", groupe.Architecture);
        Assert.True(groupe.VariantePartielle);
        Assert.Contains("RevueHumaineObligatoire", groupe.Blocages);
        Assert.Equal("VersionAnterieure", Assert.Single(groupe.Artefacts).Role);
    }

    [Fact]
    public void Version_de_contrat_inconnue_est_refusee()
    {
        using var document = JsonDocument.Parse("{\"VersionContrat\":\"future/v9\"}");

        Assert.Throws<InvalidDataException>(() =>
            new LecteurRapportVersionsUi().Lire(document.RootElement));
    }
}
