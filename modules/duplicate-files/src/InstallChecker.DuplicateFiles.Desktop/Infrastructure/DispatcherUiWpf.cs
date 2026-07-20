using System.Windows;

namespace InstallChecker.DuplicateFiles.Desktop.Infrastructure;

public sealed class DispatcherUiWpf : IUiDispatcher
{
    public Task ExecuterAsync(Action action) =>
        Application.Current.Dispatcher.InvokeAsync(action).Task;
}
