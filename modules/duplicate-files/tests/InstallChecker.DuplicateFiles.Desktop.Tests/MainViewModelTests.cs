using System.Text.Json;
using InstallChecker.DuplicateFiles.Desktop.Adaptateurs;
using InstallChecker.DuplicateFiles.Desktop.Bibliotheque;
using InstallChecker.DuplicateFiles.Desktop.Infrastructure;
using InstallChecker.DuplicateFiles.Desktop.Presentation;
using InstallChecker.DuplicateFiles.Desktop.Session;
using InstallChecker.DuplicateFiles.Desktop.ViewModels;

namespace InstallChecker.DuplicateFiles.Desktop.Tests;

public sealed class MainViewModelTests
{
    [Fact]
    public async Task Ouvrir_base_calcule_les_deux_onglets_et_sauvegarde_la_session()
    {
        var fixture = new Fixture();

        await fixture.ViewModel.OuvrirBaseAsync("bibliotheque.db");

        Assert.Single(fixture.ViewModel.GroupesDoublons);
        Assert.Single(fixture.ViewModel.GroupesVersions);
        Assert.True(fixture.ViewModel.VersionsDisponibles);
        Assert.Equal("Session sauvegardée", fixture.ViewModel.Etat);
        Assert.Equal(1, fixture.Stockage.NombreSauvegardes);
    }

    [Fact]
    public async Task Rescan_reapplique_les_decisions_stables_et_tourne_archive()
    {
        var fixture = new Fixture();
        await fixture.ViewModel.OuvrirBaseAsync("bibliotheque.db");
        fixture.ViewModel.Racines.Add(@"C:\Corpus");
        await fixture.ViewModel.ChangerDecisionAsync(
            "exact:sha256:abc",
            EtatRevueUi.Prevoir);

        await fixture.ViewModel.ScannerAsync();

        Assert.Equal(
            EtatRevueUi.Prevoir,
            fixture.ViewModel.GroupesDoublons.Single().EtatRevue);
        Assert.True(fixture.Stockage.DernierAppelTournerArchive);
    }

    [Fact]
    public async Task Scan_partiel_ne_tourne_pas_archive()
    {
        var fixture = new Fixture(scanPartiel: true);
        await fixture.ViewModel.OuvrirBaseAsync("bibliotheque.db");
        fixture.ViewModel.Racines.Add(@"C:\Corpus");

        await fixture.ViewModel.ScannerAsync();

        Assert.False(fixture.Stockage.DernierAppelTournerArchive);
        Assert.Contains("partiel", fixture.ViewModel.Etat, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Supprimer_est_toujours_inexecutable()
    {
        var fixture = new Fixture();

        Assert.False(fixture.ViewModel.SupprimerCommand.CanExecute(null));
    }

    private sealed class Fixture
    {
        public Fixture(bool scanPartiel = false)
        {
            Stockage = new FauxStockage();
            var analyseur = new FauxAnalyseur(RapportDoublons(), RapportVersions());
            var scanner = new FauxScanner(scanPartiel);
            ViewModel = new MainViewModel(
                Stockage,
                new FauxValidateur(),
                scanner,
                analyseur,
                new LecteurRapportDoublonsUi(),
                new LecteurRapportVersionsUi(),
                new FauxDialogue(),
                new DispatcherImmediat(),
                "registre");
        }

        public FauxStockage Stockage { get; }
        public MainViewModel ViewModel { get; }

        private static JsonElement RapportDoublons() => Json("""
            {
              "VersionContrat":"duplicate-files/exact-duplicates/v1",
              "Synthese":{"NombreDeGroupes":1,"NombreDeCandidatsASuppression":1,"EspaceRecuperableOctets":42},
              "Groupes":[{
                "GroupeId":"exact:sha256:abc","TailleUnitaire":42,
                "EspaceRecuperableOctets":42,"Confiance":"Certaine","ContenuSha256":"abc",
                "Exemplaires":[{
                  "FichierId":"file:1","Rang":1,"Role":"RecommandeAConserver",
                  "Fichier":{"ActeId":1,"Chemin":"C:\\a.exe","Taille":42},"Actions":[]
                }]
              }]
            }
            """);

        private static JsonElement RapportVersions() => Json("""
            {
              "VersionContrat":"duplicate-files/version-redundancy/v1",
              "Synthese":{"NombreGroupes":1,"NombreVersionsAnterieures":1},
              "Groupes":[{
                "GroupeId":"version:outil:x64","Famille":"Outil","VersionReference":"2.0",
                "Confiance":"Forte","Variante":{"Format":"exe","Architecture":"x64","Langue":null,"Partielle":false},
                "Blocages":["RevueHumaineObligatoire"],
                "Artefacts":[{"ContenuSha256":"v1","Version":"1.0","Role":"VersionAnterieure","Fichiers":[{"Chemin":"C:\\outil.exe"}],"Blocages":[]}]
              }]
            }
            """);

        private static JsonElement Json(string texte)
        {
            using var document = JsonDocument.Parse(texte);
            return document.RootElement.Clone();
        }
    }

    private sealed class FauxStockage : IStockageSessionUi
    {
        public int NombreSauvegardes { get; private set; }
        public bool DernierAppelTournerArchive { get; private set; }

        public Task<SessionDuplicateFilesUi> ChargerAsync(string chemin, CancellationToken token) =>
            throw new NotSupportedException();

        public Task SauvegarderAsync(
            string chemin,
            SessionDuplicateFilesUi session,
            bool tournerArchive,
            CancellationToken token)
        {
            NombreSauvegardes++;
            DernierAppelTournerArchive = tournerArchive;
            return Task.CompletedTask;
        }
    }

    private sealed class FauxScanner(bool partiel) : IScannerBibliotheque
    {
        public Task<ResultatScanUi> ExecuterAsync(
            BibliothequeUi bibliotheque,
            IProgress<ProgressionScanUi>? progression,
            CancellationToken token) =>
            Task.FromResult(new ResultatScanUi(
                !partiel,
                partiel,
                12,
                partiel ? [new DiagnosticUi("Partiel", "lecteur indisponible")] : []));
    }

    private sealed class FauxAnalyseur(JsonElement doublons, JsonElement versions) : IAnalyseurBibliotheque
    {
        public Task<ResultatAnalyseUi> AnalyserAsync(BibliothequeUi bibliotheque, CancellationToken token) =>
            Task.FromResult(new ResultatAnalyseUi(true, doublons, versions, []));
    }

    private sealed class FauxValidateur : IValidateurRacines
    {
        public ResultatValidationRacines Valider(IEnumerable<string> chemins) =>
            new(true, chemins.ToArray(), []);
    }

    private sealed class FauxDialogue : IDialogueFichiers
    {
        public string? ChoisirDossier(string? initial) => null;
        public string? OuvrirBase(string? initial) => null;
        public string? OuvrirJson(string? initial) => null;
        public string? SauvegarderSession(string? initial) => null;
    }

    private sealed class DispatcherImmediat : IUiDispatcher
    {
        public Task ExecuterAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }
    }
}
