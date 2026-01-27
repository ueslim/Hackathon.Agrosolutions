using FIAP.AgroSolutions.SensorIngestion.Infrastructure.Messaging;
using FIAP.AgroSolutions.SensorIngestion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



namespace FIAP.AgroSolutions.SensorIngestion.Infrastructure.Outbox;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly RabbitMQHelper _rabbit;
    private readonly string _queueName;

    public OutboxPublisher(
        IServiceProvider provider,
        ILogger<OutboxPublisher> logger,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _provider = provider;
        _logger = logger;

        var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        var port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var userName = configuration["RabbitMQ:UserName"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";

        _queueName = configuration["Queues:SensorReadings"] ?? "sensor.readings";

        var rabbitLogger = loggerFactory.CreateLogger<RabbitMQHelper>();
        _rabbit = new RabbitMQHelper(hostName, port, userName, password, rabbitLogger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _rabbit.EnsureQueueExists(_queueName);
            _logger.LogInformation("OutboxPublisher conectado ao RabbitMQ. Queue={Queue}", _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha conectando ao RabbitMQ. Mensagens acumularão no Outbox.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();

                var pending = await db.OutboxMessages
                    .Where(x => x.ProcessedAtUtc == null)
                    .OrderBy(x => x.OccurredAtUtc)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                if (pending.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                foreach (var msg in pending)
                {
                    try
                    {
                        _rabbit.Publish(_queueName, msg.Payload);
                        msg.ProcessedAtUtc = DateTime.UtcNow;
                        msg.LastError = null;
                    }
                    catch (Exception ex)
                    {
                        msg.AttemptCount += 1;
                        msg.LastError = ex.Message;
                        _logger.LogError(ex, "Falha publicando outbox {Id} (tentativas={Attempts})", msg.Id, msg.AttemptCount);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no loop do OutboxPublisher");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    public override void Dispose()
    {
        _rabbit?.Dispose();
        base.Dispose();
    }
}
