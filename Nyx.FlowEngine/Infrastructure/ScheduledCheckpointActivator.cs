using Nyx.FlowEngine.Infrastructure;

namespace Nyx.FlowEngine.Infrastructure;

public class ScheduledCheckpointActivator : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ScheduledCheckpointActivator> _logger;

    public ScheduledCheckpointActivator(IServiceProvider services, ILogger<ScheduledCheckpointActivator> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("ScheduledCheckpointActivator background service started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IFlowRepository>();
                await repo.ActivateDueScheduledCheckpointsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating due scheduled checkpoints.");
            }

            // Check every 5 minutes
            await Task.Delay(TimeSpan.FromMinutes(5), ct);
        }
    }
}
