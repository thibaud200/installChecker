namespace InstallChecker.DuplicateFiles.Desktop.Infrastructure;

public interface IUiDispatcher
{
    Task ExecuterAsync(Action action);
}
