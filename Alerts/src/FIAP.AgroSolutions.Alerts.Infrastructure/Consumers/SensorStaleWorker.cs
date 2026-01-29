using FIAP.AgroSolutions.Alerts.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Consumers;

/// <summary>
/// Worker agendado que roda periodicamente para detectar "sensor parado"
/// (quando não chega nenhuma leitura por X minutos).
///
/// Por que precisamos disso?
/// - O AlertEngine só processa regras quando chega leitura.
/// - "SensorStale" é justamente a ausência de leitura, então não haveria evento para disparar.
/// - Esse worker faz a verificação por tempo (polling) e cria o alerta quando necessário.
/// </summary>
public class SensorStaleWorker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<SensorStaleWorker> _logger;

    public SensorStaleWorker(IServiceProvider provider, ILogger<SensorStaleWorker> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <summary>
    /// Loop principal do worker:
    /// - executa o SensorStaleService
    /// - espera 1 minuto
    /// - repete até o serviço ser cancelado (shutdown da aplicação).
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SensorStaleWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Cria um escopo por execução para garantir DI correto (DbContext, etc.)
                using var scope = _provider.CreateScope();

                // Serviço que contém a regra de "sensor parado"
                var svc = scope.ServiceProvider.GetRequiredService<SensorStaleService>();

                // Roda a verificação e cria alertas se necessário
                await svc.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // O worker não pode morrer por exceção: loga e continua no próximo ciclo
                _logger.LogError(ex, "Erro no SensorStaleWorker");
            }

            // Intervalo do polling (pode ser movido para config se quiser)
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
