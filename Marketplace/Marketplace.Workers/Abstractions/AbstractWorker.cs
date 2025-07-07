using Marketplace.Workers.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marketplace.Workers.Abstractions;

public abstract class AbstractWorker(WorkerSettings settings, ILogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Starting worker \"{name}\". Work interval is {amount} hours. Starts immediately: {val}",
            settings.Name, settings.IterationIntervalHours, settings.StartImmediately);
        
        var interval = TimeSpan.FromHours(settings.IterationIntervalHours);
        var timer = new PeriodicTimer(interval);
        do
        {
            if (!settings.StartImmediately)
                continue;
            logger.LogDebug("Worker \"{name}\" starts new work iteration", settings.Name);
            
            await PerformIterationAsync(stoppingToken);
            
            logger.LogDebug("Worker \"{name}\" completed work iteration", settings.Name);
        }
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken));
        
        logger.LogDebug("Stopping worker \"{name}\"", settings.Name);
    }

    protected abstract Task PerformIterationAsync(CancellationToken stoppingToken);
}
