using InstallChecker.DuplicateFiles.Desktop.Infrastructure;

namespace InstallChecker.DuplicateFiles.Desktop.Tests;

public sealed class InfrastructureMvvmTests
{
    [Fact]
    public void RelayCommand_execute_laction_et_respecte_CanExecute()
    {
        var executions = 0;
        var autorisee = false;
        var commande = new RelayCommand(() => executions++, () => autorisee);

        Assert.False(commande.CanExecute(null));

        autorisee = true;
        commande.Execute(null);

        Assert.Equal(1, executions);
    }

    [Fact]
    public async Task AsyncRelayCommand_refuse_une_double_execution()
    {
        var fin = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var executions = 0;
        var commande = new AsyncRelayCommand(async () =>
        {
            executions++;
            await fin.Task;
        });

        commande.Execute(null);
        commande.Execute(null);

        Assert.Equal(1, executions);
        Assert.True(commande.EstEnCours);

        fin.SetResult();
        await commande.ExecutionCourante;

        Assert.False(commande.EstEnCours);
    }
}
