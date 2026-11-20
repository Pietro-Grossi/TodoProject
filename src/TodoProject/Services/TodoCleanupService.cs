namespace TodoProject.Services;

public class TodoCleanupService(
    TodoManager manager,
    ILogger<TodoCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                int deletedCount = manager.RemoveCompletedTodoItems();
                logger.LogInformation(
                    "Pulizia periodica completata con {DeletedCount} task eliminati.", deletedCount);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // L'applicazione ha richiesto l'arresto del servizio.
        }
    }
}
