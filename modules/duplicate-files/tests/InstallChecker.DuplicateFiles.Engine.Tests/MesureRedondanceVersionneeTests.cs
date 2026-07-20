using System.Diagnostics;
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Observations;
using Xunit.Abstractions;

namespace InstallChecker.DuplicateFiles.Tests;

public class MesureRedondanceVersionneeTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Performance")]
    public void Mesure_100000_observations_en_10000_familles()
    {
        const int nombreFamilles = 10_000;
        const int versionsParFamille = 10;
        var actes = new List<ActeObservation>(nombreFamilles * versionsParFamille);
        var contextes = new List<ContexteObservation>(nombreFamilles * versionsParFamille);
        long id = 0;
        for (var famille = 0; famille < nombreFamilles; famille++)
        {
            for (var version = 1; version <= versionsParFamille; version++)
            {
                id++;
                actes.Add(new ActeObservation(
                    id,
                    100,
                    id.ToString("x64"),
                    new Dictionary<Attribut, ValeurObservee>()));
                contextes.Add(new ContexteObservation(
                    id,
                    $@"C:\perf\outil{famille:D5}-{version}.zip",
                    "2026-01-01T00:00:00.0000000Z"));
            }
        }

        var omega = new OmegaDeMesure(new ModeleObservations(actes), contextes);
        var allocationAvant = GC.GetTotalAllocatedBytes(precise: true);
        var chronometre = Stopwatch.StartNew();

        var rapport = GenerateurRedondanceVersionnee.Generer(omega);

        chronometre.Stop();
        var allocation = GC.GetTotalAllocatedBytes(precise: true) - allocationAvant;
        output.WriteLine($"Durée: {chronometre.Elapsed.TotalSeconds:F3} s");
        output.WriteLine($"Allocation: {allocation / (1024d * 1024d):F1} MiB");
        Assert.Equal(VersionsContratDuplicateFiles.RedondanceVersionneeV1, rapport.VersionContrat);
        Assert.Equal(nombreFamilles, rapport.Synthese.NombreGroupes);
        Assert.Equal(90_000, rapport.Synthese.NombreVersionsAnterieures);
    }

    private sealed class OmegaDeMesure(
        ModeleObservations modele,
        IReadOnlyList<ContexteObservation> contextes) : IObservationsSource
    {
        public ModeleObservations ProjeterModele() => modele;
        public IReadOnlyList<ContexteObservation> ProjeterContexte() => contextes;
        public IndexOmega ProjeterIdentite() => throw new NotSupportedException();
    }
}
