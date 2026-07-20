using System.Text.Json;
using System.Text.Json.Serialization;
using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class ContratRedondanceVersionneeTests
{
    [Fact]
    public void Le_contrat_F1_serialise_les_valeurs_fermees_en_chaines_sans_action_destructive()
    {
        var rapport = new RapportRedondanceVersionnee(
            VersionsContratDuplicateFiles.RedondanceVersionneeV1,
            new SourceRapportVersionnee(2),
            new SyntheseRedondanceVersionnee(1, 2, 1, 1, 0),
            [new GroupeRedondanceVersionnee(
                "version:sha256:" + new string('a', 64),
                CategorieRedondanceVersionnee.VersionRedundancyCandidate,
                "Outil",
                new VarianteVersionnee(".zip", null, null, null, null, false),
                NiveauConfianceVersionnee.Faible,
                "2",
                [new ArtefactVersionneRapporte(
                    new string('b', 64),
                    [new FichierVersionneRapporte("file:sha256:" + new string('c', 64), @"C:\outil-1.zip", 10)],
                    "1",
                    RoleComparaisonVersionnee.VersionAnterieure,
                    [],
                    [],
                    [ActionVersionnee.Examiner, ActionVersionnee.Ignorer],
                    [RaisonBlocageVersionnee.RevueHumaineObligatoire,
                     RaisonBlocageVersionnee.SuppressionAutomatiqueInterdite])],
                [RaisonBlocageVersionnee.RevueHumaineObligatoire])],
            new Dictionary<MotifExclusionVersionnee, int>
            {
                [MotifExclusionVersionnee.AucuneVersion] = 3,
            });
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
        };

        var json = JsonSerializer.Serialize(rapport, options);

        Assert.Contains("\"VersionContrat\":\"duplicate-files/version-redundancy/v1\"", json);
        Assert.Contains("\"Categorie\":\"VersionRedundancyCandidate\"", json);
        Assert.Contains("\"Role\":\"VersionAnterieure\"", json);
        Assert.Contains("\"Examiner\"", json);
        Assert.Contains("\"Ignorer\"", json);
        Assert.DoesNotContain("Supprimer", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Corbeille", json, StringComparison.OrdinalIgnoreCase);
    }
}
