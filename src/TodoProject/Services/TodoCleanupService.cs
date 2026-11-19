namespace TodoProject.Services;

public class TodoCleanupService(TodoManager manager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                manager.RemoveCompletedTodoItems();
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // L'applicazione ha richiesto l'arresto del servizio.
        }
    }
}