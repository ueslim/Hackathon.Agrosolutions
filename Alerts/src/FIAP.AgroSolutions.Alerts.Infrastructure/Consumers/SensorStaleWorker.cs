using FIAP.AgroSolutions.Alerts.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Consumers;

public class SensorStaleWorker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<SensorStaleWorker> _logger;

    public SensorStaleWorker(IServiceProvider provider, ILogger<SensorStaleWorker> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SensorStaleWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _provider.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<SensorStaleService>();
                await svc.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no SensorStaleWorker");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
