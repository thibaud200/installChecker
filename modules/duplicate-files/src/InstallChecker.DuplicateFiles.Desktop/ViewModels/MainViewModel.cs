using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using InstallChecker.DuplicateFiles.Desktop.Adaptateurs;
using InstallChecker.DuplicateFiles.Desktop.Bibliotheque;
using InstallChecker.DuplicateFiles.Desktop.Infrastructure;
using InstallChecker.DuplicateFiles.Desktop.Presentation;
using InstallChecker.DuplicateFiles.Desktop.Session;

namespace InstallChecker.DuplicateFiles.Desktop.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IStockageSessionUi _stockage;
    private readonly IValidateurRacines _validateurRacines;
    private readonly IScannerBibliotheque _scanner;
    private readonly IAnalyseurBibliotheque _analyseur;
    private readonly LecteurRapportDoublonsUi _lecteurDoublons;
    private readonly LecteurRapportVersionsUi _lecteurVersions;
    private readonly IDialogueFichiers _dialogues;
    private readonly IUiDispatcher _dispatcher;
    private readonly string _cheminRegistre;
    private readonly Dictionary<string, DecisionRevueUi> _decisions =
        new(StringComparer.Ordinal);
    private BibliothequeUi? _bibliotheque;
    private JsonElement? _rapportDoublons;
    private JsonElement? _rapportVersions;
    private bool _estOccupe;
    private bool _versionsDisponibles;
    private long _fichiersTraites;
    private string _cheminCourant = string.Empty;
    private string _etat = "Prêt";
    private string _messageVersions = "Aucune analyse chargée";
    private string? _racineSelectionnee;
    private GroupeDoublonViewModel? _groupeDoublonSelectionne;
    private GroupeVersionViewModel? _groupeVersionSelectionne;
    private string _rechercheDoublons = string.Empty;
    private string _rechercheVersions = string.Empty;
    private int _nombreCandidats;
    private long _espaceRecuperable;
    private int _nombreVersionsAnterieures;

    public MainViewModel(
        IStockageSessionUi stockage,
        IValidateurRacines validateurRacines,
        IScannerBibliotheque scanner,
        IAnalyseurBibliotheque analyseur,
        LecteurRapportDoublonsUi lecteurDoublons,
        LecteurRapportVersionsUi lecteurVersions,
        IDialogueFichiers dialogues,
        IUiDispatcher dispatcher,
        string cheminRegistre)
    {
        _stockage = stockage;
        _validateurRacines = validateurRacines;
        _scanner = scanner;
        _analyseur = analyseur;
        _lecteurDoublons = lecteurDoublons;
        _lecteurVersions = lecteurVersions;
        _dialogues = dialogues;
        _dispatcher = dispatcher;
        _cheminRegistre = cheminRegistre;

        ScannerCommand = new AsyncRelayCommand(ScannerAsync, () => !EstOccupe && _bibliotheque is not null);
        OuvrirBaseCommand = new AsyncRelayCommand(OuvrirBaseDepuisDialogueAsync, () => !EstOccupe);
        OuvrirSessionCommand = new AsyncRelayCommand(OuvrirSessionDepuisDialogueAsync, () => !EstOccupe);
        ImporterRapportCommand = new AsyncRelayCommand(ImporterRapportDepuisDialogueAsync, () => !EstOccupe);
        AjouterRacineCommand = new RelayCommand(AjouterRacine, () => !EstOccupe);
        RetirerRacineCommand = new RelayCommand(RetirerRacine, () => !EstOccupe && RacineSelectionnee is not null);
        SupprimerCommand = new RelayCommand(() => { }, () => false);
    }

    public ObservableCollection<string> Racines { get; } = [];
    public ObservableCollection<GroupeDoublonViewModel> GroupesDoublons { get; } = [];
    public ObservableCollection<GroupeVersionViewModel> GroupesVersions { get; } = [];
    public ObservableCollection<GroupeDoublonViewModel> GroupesDoublonsFiltres { get; } = [];
    public ObservableCollection<GroupeVersionViewModel> GroupesVersionsFiltres { get; } = [];
    public ObservableCollection<DiagnosticUi> Diagnostics { get; } = [];
    public ObservableCollection<string> DecisionsIntrouvables { get; } = [];
    public IReadOnlyList<EtatRevueUi> EtatsRevue { get; } = Enum.GetValues<EtatRevueUi>();

    public AsyncRelayCommand ScannerCommand { get; }
    public AsyncRelayCommand OuvrirBaseCommand { get; }
    public AsyncRelayCommand OuvrirSessionCommand { get; }
    public AsyncRelayCommand ImporterRapportCommand { get; }
    public RelayCommand AjouterRacineCommand { get; }
    public RelayCommand RetirerRacineCommand { get; }
    public RelayCommand SupprimerCommand { get; }

    public bool EstOccupe
    {
        get => _estOccupe;
        private set
        {
            if (!Set(ref _estOccupe, value))
                return;

            ScannerCommand.RaiseCanExecuteChanged();
            OuvrirBaseCommand.RaiseCanExecuteChanged();
            OuvrirSessionCommand.RaiseCanExecuteChanged();
            ImporterRapportCommand.RaiseCanExecuteChanged();
            AjouterRacineCommand.RaiseCanExecuteChanged();
            RetirerRacineCommand.RaiseCanExecuteChanged();
        }
    }

    public bool VersionsDisponibles
    {
        get => _versionsDisponibles;
        private set => Set(ref _versionsDisponibles, value);
    }

    public long FichiersTraites
    {
        get => _fichiersTraites;
        private set => Set(ref _fichiersTraites, value);
    }

    public string CheminCourant
    {
        get => _cheminCourant;
        private set => Set(ref _cheminCourant, value);
    }

    public string Etat
    {
        get => _etat;
        private set => Set(ref _etat, value);
    }

    public string MessageVersions
    {
        get => _messageVersions;
        private set => Set(ref _messageVersions, value);
    }

    public string? RacineSelectionnee
    {
        get => _racineSelectionnee;
        set
        {
            if (Set(ref _racineSelectionnee, value))
                RetirerRacineCommand.RaiseCanExecuteChanged();
        }
    }

    public GroupeDoublonViewModel? GroupeDoublonSelectionne
    {
        get => _groupeDoublonSelectionne;
        set => Set(ref _groupeDoublonSelectionne, value);
    }

    public GroupeVersionViewModel? GroupeVersionSelectionne
    {
        get => _groupeVersionSelectionne;
        set => Set(ref _groupeVersionSelectionne, value);
    }

    public string RechercheDoublons
    {
        get => _rechercheDoublons;
        set
        {
            if (Set(ref _rechercheDoublons, value))
                ActualiserDoublonsFiltres();
        }
    }

    public string RechercheVersions
    {
        get => _rechercheVersions;
        set
        {
            if (Set(ref _rechercheVersions, value))
                ActualiserVersionsFiltrees();
        }
    }

    public int NombreCandidats
    {
        get => _nombreCandidats;
        private set => Set(ref _nombreCandidats, value);
    }

    public long EspaceRecuperable
    {
        get => _espaceRecuperable;
        private set => Set(ref _espaceRecuperable, value);
    }

    public int NombreVersionsAnterieures
    {
        get => _nombreVersionsAnterieures;
        private set => Set(ref _nombreVersionsAnterieures, value);
    }

    public async Task OuvrirBaseAsync(string cheminBase)
    {
        EstOccupe = true;
        Etat = "Analyse de la bibliothèque";
        try
        {
            var cheminComplet = Path.GetFullPath(cheminBase);
            _bibliotheque = new BibliothequeUi(
                Path.GetFileNameWithoutExtension(cheminComplet),
                cheminComplet,
                _cheminRegistre,
                Racines.ToArray(),
                Path.ChangeExtension(cheminComplet, ".session.json"));
            ScannerCommand.RaiseCanExecuteChanged();

            var resultat = await _analyseur.AnalyserAsync(_bibliotheque, default);
            await RemplacerDiagnosticsAsync(resultat.Diagnostics);
            if (!resultat.Reussi)
            {
                Etat = "Analyse incomplète";
                return;
            }

            await AppliquerRapportsAsync(resultat.RapportDoublons, resultat.RapportVersions);
            await SauvegarderSessionAsync(tournerArchive: false);
            Etat = "Session sauvegardée";
        }
        finally
        {
            EstOccupe = false;
        }
    }

    public async Task OuvrirSessionAsync(string cheminSession)
    {
        EstOccupe = true;
        Etat = "Ouverture de la session";
        try
        {
            var session = await _stockage.ChargerAsync(cheminSession, default);
            _bibliotheque = session.Bibliotheque with { CheminSession = Path.GetFullPath(cheminSession) };
            _decisions.Clear();
            foreach (var decision in session.Decisions)
                _decisions[decision.Key] = decision.Value;

            await _dispatcher.ExecuterAsync(() =>
            {
                Racines.Clear();
                foreach (var racine in _bibliotheque.Racines)
                    Racines.Add(racine);
                RechercheDoublons = session.FiltresDoublons.Recherche;
                RechercheVersions = session.FiltresVersions.Recherche;
            });
            await RemplacerDiagnosticsAsync(session.Diagnostics);
            await AppliquerRapportsAsync(session.RapportDoublons, session.RapportVersions);
            ScannerCommand.RaiseCanExecuteChanged();
            Etat = "Session chargée";
        }
        finally
        {
            EstOccupe = false;
        }
    }

    public async Task ImporterRapportAsync(string cheminRapport)
    {
        EstOccupe = true;
        Etat = "Import du rapport";
        try
        {
            var json = await _lecteurDoublons.ChargerJsonAsync(cheminRapport, default);
            _ = await Task.Run(() => _lecteurDoublons.Lire(json));
            var cheminComplet = Path.GetFullPath(cheminRapport);
            _bibliotheque = new BibliothequeUi(
                Path.GetFileNameWithoutExtension(cheminComplet),
                string.Empty,
                _cheminRegistre,
                [],
                Path.ChangeExtension(cheminComplet, ".session.json"));
            _decisions.Clear();
            await AppliquerRapportsAsync(json, null);
            await SauvegarderSessionAsync(tournerArchive: false);
            Etat = "Rapport importé et session sauvegardée";
        }
        finally
        {
            EstOccupe = false;
        }
    }

    public async Task ScannerAsync()
    {
        if (_bibliotheque is null)
            return;

        var validation = _validateurRacines.Valider(Racines);
        await RemplacerDiagnosticsAsync(validation.Diagnostics);
        if (!validation.EstValide)
        {
            Etat = "Racines à corriger";
            return;
        }

        EstOccupe = true;
        FichiersTraites = 0;
        Etat = "Scan en cours";
        _bibliotheque = _bibliotheque with { Racines = validation.Racines };
        try
        {
            var progression = new ProgressionSimple(p =>
            {
                _ = _dispatcher.ExecuterAsync(() =>
                {
                    FichiersTraites = p.FichiersTraites;
                    CheminCourant = p.CheminCourant;
                });
            });
            var scan = await _scanner.ExecuterAsync(_bibliotheque, progression, default);
            FichiersTraites = scan.FichiersTraites;
            await RemplacerDiagnosticsAsync(scan.Diagnostics);
            if (!scan.Reussi || scan.Partiel)
            {
                Etat = scan.Partiel
                    ? "Scan partiel : session actuelle conservée"
                    : "Scan échoué : session actuelle conservée";
                return;
            }

            Etat = "Analyse des résultats";
            var analyse = await _analyseur.AnalyserAsync(_bibliotheque, default);
            await RemplacerDiagnosticsAsync(analyse.Diagnostics);
            if (!analyse.Reussi)
            {
                Etat = "Analyse échouée : session actuelle conservée";
                return;
            }

            await AppliquerRapportsAsync(analyse.RapportDoublons, analyse.RapportVersions);
            await SauvegarderSessionAsync(tournerArchive: true);
            Etat = "Scan terminé, session archivée";
        }
        finally
        {
            EstOccupe = false;
            CheminCourant = string.Empty;
        }
    }

    public async Task ChangerDecisionAsync(string groupeId, EtatRevueUi etat)
    {
        _decisions[groupeId] = new DecisionRevueUi(
            groupeId,
            null,
            etat,
            DateTimeOffset.UtcNow);

        var groupeDoublon = GroupesDoublons.FirstOrDefault(g => g.GroupeId == groupeId);
        if (groupeDoublon is not null)
            groupeDoublon.AppliquerEtat(etat);
        var groupeVersion = GroupesVersions.FirstOrDefault(g => g.GroupeId == groupeId);
        if (groupeVersion is not null)
            groupeVersion.AppliquerEtat(etat);

        if (_bibliotheque is not null)
        {
            await SauvegarderSessionAsync(tournerArchive: false);
            Etat = "Décision sauvegardée";
        }
    }

    private async Task AppliquerRapportsAsync(JsonElement? doublonsJson, JsonElement? versionsJson)
    {
        var rapportDoublons = doublonsJson is not null
            ? await Task.Run(() => _lecteurDoublons.Lire(doublonsJson.Value))
            : null;
        var rapportVersions = versionsJson is not null
            ? await Task.Run(() => _lecteurVersions.Lire(versionsJson.Value))
            : null;

        _rapportDoublons = doublonsJson?.Clone();
        _rapportVersions = versionsJson?.Clone();

        await _dispatcher.ExecuterAsync(() =>
        {
            GroupesDoublons.Clear();
            if (rapportDoublons is not null)
            {
                foreach (var groupe in rapportDoublons.Groupes)
                {
                    GroupesDoublons.Add(new GroupeDoublonViewModel(
                        groupe,
                        EtatPour(groupe.GroupeId),
                        ChangerDecisionAsync));
                }
                NombreCandidats = rapportDoublons.NombreCandidats;
                EspaceRecuperable = rapportDoublons.EspaceRecuperable;
            }

            GroupesVersions.Clear();
            if (rapportVersions is not null)
            {
                foreach (var groupe in rapportVersions.Groupes)
                {
                    GroupesVersions.Add(new GroupeVersionViewModel(
                        groupe,
                        EtatPour(groupe.GroupeId),
                        ChangerDecisionAsync));
                }
                NombreVersionsAnterieures = rapportVersions.NombreVersionsAnterieures;
            }
            else
            {
                NombreVersionsAnterieures = 0;
            }

            VersionsDisponibles = rapportVersions is not null;
            MessageVersions = VersionsDisponibles
                ? string.Empty
                : "Cette source ne contient pas de rapport de versions apparentées.";
            GroupeDoublonSelectionne = GroupesDoublons.FirstOrDefault();
            GroupeVersionSelectionne = GroupesVersions.FirstOrDefault();
            ActualiserDoublonsFiltres();
            ActualiserVersionsFiltrees();

            var idsCourants = GroupesDoublons.Select(g => g.GroupeId)
                .Concat(GroupesVersions.Select(g => g.GroupeId))
                .ToHashSet(StringComparer.Ordinal);
            DecisionsIntrouvables.Clear();
            foreach (var id in _decisions.Keys.Where(id => !idsCourants.Contains(id)).Order())
                DecisionsIntrouvables.Add(id);
        });
    }

    private EtatRevueUi EtatPour(string groupeId) =>
        _decisions.TryGetValue(groupeId, out var decision)
            ? decision.Etat
            : EtatRevueUi.AExaminer;

    private Task SauvegarderSessionAsync(bool tournerArchive)
    {
        if (_bibliotheque is null)
            return Task.CompletedTask;

        var session = new SessionDuplicateFilesUi(
            VersionsContratUi.SessionV1,
            _bibliotheque with { Racines = Racines.ToArray() },
            DateTimeOffset.UtcNow,
            _rapportDoublons?.Clone(),
            _rapportVersions?.Clone(),
            new Dictionary<string, DecisionRevueUi>(_decisions, StringComparer.Ordinal),
            new EtatFiltresUi(RechercheDoublons, null, null),
            new EtatFiltresUi(RechercheVersions, null, null),
            Diagnostics.ToArray());
        return _stockage.SauvegarderAsync(
            _bibliotheque.CheminSession,
            session,
            tournerArchive,
            default);
    }

    private Task RemplacerDiagnosticsAsync(IEnumerable<DiagnosticUi> diagnostics) =>
        _dispatcher.ExecuterAsync(() =>
        {
            Diagnostics.Clear();
            foreach (var diagnostic in diagnostics)
                Diagnostics.Add(diagnostic);
        });

    private async Task OuvrirBaseDepuisDialogueAsync()
    {
        var chemin = _dialogues.OuvrirBase(_bibliotheque?.CheminBase);
        if (chemin is not null)
            await OuvrirBaseAsync(chemin);
    }

    private async Task OuvrirSessionDepuisDialogueAsync()
    {
        var chemin = _dialogues.OuvrirJson(_bibliotheque?.CheminSession);
        if (chemin is not null)
            await OuvrirSessionAsync(chemin);
    }

    private async Task ImporterRapportDepuisDialogueAsync()
    {
        var chemin = _dialogues.OuvrirJson(null);
        if (chemin is not null)
            await ImporterRapportAsync(chemin);
    }

    private void AjouterRacine()
    {
        var chemin = _dialogues.ChoisirDossier(RacineSelectionnee);
        if (chemin is null || Racines.Contains(chemin, StringComparer.OrdinalIgnoreCase))
            return;

        Racines.Add(chemin);
        RacineSelectionnee = chemin;
    }

    private void RetirerRacine()
    {
        if (RacineSelectionnee is null)
            return;

        Racines.Remove(RacineSelectionnee);
        RacineSelectionnee = Racines.FirstOrDefault();
    }

    private bool FiltrerDoublon(GroupeDoublonViewModel groupe)
    {
        if (string.IsNullOrWhiteSpace(RechercheDoublons))
            return true;

        var recherche = RechercheDoublons.Trim();
        return groupe.GroupeId.Contains(recherche, StringComparison.OrdinalIgnoreCase)
            || (groupe.Sha256?.Contains(recherche, StringComparison.OrdinalIgnoreCase) ?? false)
            || groupe.Fichiers.Any(f => f.Chemin.Contains(recherche, StringComparison.OrdinalIgnoreCase));
    }

    private bool FiltrerVersion(GroupeVersionViewModel groupe)
    {
        if (string.IsNullOrWhiteSpace(RechercheVersions))
            return true;

        var recherche = RechercheVersions.Trim();
        return groupe.GroupeId.Contains(recherche, StringComparison.OrdinalIgnoreCase)
            || groupe.Famille.Contains(recherche, StringComparison.OrdinalIgnoreCase)
            || groupe.VersionReference.Contains(recherche, StringComparison.OrdinalIgnoreCase)
            || (groupe.Architecture?.Contains(recherche, StringComparison.OrdinalIgnoreCase) ?? false)
            || groupe.Artefacts.Any(a => a.Version.Contains(recherche, StringComparison.OrdinalIgnoreCase));
    }

    private void ActualiserDoublonsFiltres()
    {
        GroupesDoublonsFiltres.Clear();
        foreach (var groupe in GroupesDoublons.Where(FiltrerDoublon))
            GroupesDoublonsFiltres.Add(groupe);
    }

    private void ActualiserVersionsFiltrees()
    {
        GroupesVersionsFiltres.Clear();
        foreach (var groupe in GroupesVersions.Where(FiltrerVersion))
            GroupesVersionsFiltres.Add(groupe);
    }

    private sealed class ProgressionSimple(Action<ProgressionScanUi> rapporter)
        : IProgress<ProgressionScanUi>
    {
        public void Report(ProgressionScanUi value) => rapporter(value);
    }
}
