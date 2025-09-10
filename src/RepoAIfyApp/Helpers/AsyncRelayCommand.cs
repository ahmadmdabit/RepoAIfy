using System.Windows.Input;

using Serilog;

namespace RepoAIfyApp.Helpers;

public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> execute;
    private readonly Predicate<object?>? canExecute;
    private bool isExecuting;

    public AsyncRelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter)
    {
        return !isExecuting && (canExecute == null || canExecute(parameter));
    }

    public async void Execute(object? parameter)
    {
        isExecuting = true;
        CommandManager.InvalidateRequerySuggested();
        try
        {
            await execute(parameter);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred in an async command.");
            // Optionally, display a message to the user.
        }
        finally
        {
            isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}