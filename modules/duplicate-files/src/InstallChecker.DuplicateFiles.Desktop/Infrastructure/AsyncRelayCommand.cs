using System.Windows.Input;

namespace InstallChecker.DuplicateFiles.Desktop.Infrastructure;

public sealed class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    : ObservableObject, ICommand
{
    private bool _estEnCours;

    public event EventHandler? CanExecuteChanged;

    public bool EstEnCours
    {
        get => _estEnCours;
        private set => Set(ref _estEnCours, value);
    }

    public Task ExecutionCourante { get; private set; } = Task.CompletedTask;

    public bool CanExecute(object? parameter) =>
        !EstEnCours && (canExecute?.Invoke() ?? true);

    public void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        ExecutionCourante = ExecuterAsync();
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private async Task ExecuterAsync()
    {
        EstEnCours = true;
        RaiseCanExecuteChanged();

        try
        {
            await execute();
        }
        finally
        {
            EstEnCours = false;
            RaiseCanExecuteChanged();
        }
    }
}
