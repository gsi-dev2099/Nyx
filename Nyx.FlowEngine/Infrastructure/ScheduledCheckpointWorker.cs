namespace Nyx.FlowEngine.Infrastructure;

public class ScheduledCheckpointWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ScheduledCheckpointWorker> _logger;

    public ScheduledCheckpointWorker(IServiceProvider services, ILogger<ScheduledCheckpointWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Nyx Flow Engine: ScheduledCheckpointWorker iniciado (Chequeo cada 60s).");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ICycleRepository>();
                await repo.ActivateDueScheduledCheckpointsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ScheduledCheckpointWorker al activar checkpoints programados.");
            }

            // Chequeo periódico cada 60 segundos
            await Task.Delay(TimeSpan.FromSeconds(60), ct);
        }
    }
}
