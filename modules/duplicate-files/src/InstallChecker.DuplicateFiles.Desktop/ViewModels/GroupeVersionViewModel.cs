using InstallChecker.DuplicateFiles.Desktop.Infrastructure;
using InstallChecker.DuplicateFiles.Desktop.Presentation;
using InstallChecker.DuplicateFiles.Desktop.Session;

namespace InstallChecker.DuplicateFiles.Desktop.ViewModels;

public sealed class GroupeVersionViewModel : ObservableObject
{
    private readonly Func<string, EtatRevueUi, Task>? _changerDecision;
    private EtatRevueUi _etatRevue;

    public GroupeVersionViewModel(
        GroupeVersionUi groupe,
        EtatRevueUi etatRevue,
        Func<string, EtatRevueUi, Task>? changerDecision = null)
    {
        Groupe = groupe;
        _etatRevue = etatRevue;
        _changerDecision = changerDecision;
    }

    public GroupeVersionUi Groupe { get; }
    public string GroupeId => Groupe.GroupeId;
    public string Famille => Groupe.Famille;
    public string VersionReference => Groupe.VersionReference;
    public string Confiance => Groupe.Confiance;
    public string Format => Groupe.Format;
    public string? Architecture => Groupe.Architecture;
    public string? Langue => Groupe.Langue;
    public bool VariantePartielle => Groupe.VariantePartielle;
    public IReadOnlyList<string> Blocages => Groupe.Blocages;
    public IReadOnlyList<ArtefactVersionUi> Artefacts => Groupe.Artefacts;
    public bool RevueHumaine => Groupe.Blocages.Contains("RevueHumaineObligatoire");

    public EtatRevueUi EtatRevue
    {
        get => _etatRevue;
        set
        {
            if (Set(ref _etatRevue, value) && _changerDecision is not null)
                _ = _changerDecision(GroupeId, value);
        }
    }

    internal void AppliquerEtat(EtatRevueUi value) => Set(ref _etatRevue, value);
}
