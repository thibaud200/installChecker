using InstallChecker.DuplicateFiles.Desktop.Infrastructure;
using InstallChecker.DuplicateFiles.Desktop.Presentation;
using InstallChecker.DuplicateFiles.Desktop.Session;

namespace InstallChecker.DuplicateFiles.Desktop.ViewModels;

public sealed class GroupeDoublonViewModel : ObservableObject
{
    private readonly Func<string, EtatRevueUi, Task>? _changerDecision;
    private EtatRevueUi _etatRevue;

    public GroupeDoublonViewModel(
        GroupeDoublonUi groupe,
        EtatRevueUi etatRevue,
        Func<string, EtatRevueUi, Task>? changerDecision = null)
    {
        Groupe = groupe;
        _etatRevue = etatRevue;
        _changerDecision = changerDecision;
    }

    public GroupeDoublonUi Groupe { get; }
    public string GroupeId => Groupe.GroupeId;
    public long TailleUnitaire => Groupe.TailleUnitaire;
    public long EspaceRecuperable => Groupe.EspaceRecuperable;
    public string Confiance => Groupe.Confiance;
    public string? Sha256 => Groupe.Sha256;
    public IReadOnlyList<FichierDoublonUi> Fichiers => Groupe.Fichiers;
    public int NombreFichiers => Groupe.Fichiers.Count;
    public string CheminPrincipal => Groupe.Fichiers.FirstOrDefault()?.Chemin ?? string.Empty;

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
