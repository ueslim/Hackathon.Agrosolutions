using FIAP.AgroSolutions.Alerts.Application.DTOs;
using FIAP.AgroSolutions.Alerts.Application.Services;
using FIAP.AgroSolutions.Alerts.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Consumers;

public class SensorReadingConsumer : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<SensorReadingConsumer> _logger;
    private readonly IConfiguration _config;

    public SensorReadingConsumer(IServiceProvider provider, ILogger<SensorReadingConsumer> logger, IConfiguration config)
    {
        _provider = provider;
        _logger = logger;
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbit = _config.GetSection("RabbitMQ");
        var queues = _config.GetSection("Queues");

        var helper = new RabbitMQHelper(
            hostName: rabbit["HostName"] ?? "localhost",
            port: int.TryParse(rabbit["Port"], out var p) ? p : 5672,
            userName: rabbit["UserName"] ?? "guest",
            password: rabbit["Password"] ?? "guest"
        );

        var queueName = queues["SensorReadings"] ?? "sensor.readings";
        helper.EnsureQueueExists(queueName);

        // QoS: 10 msgs por worker
        helper.Channel.BasicQos(0, 10, false);

        var consumer = new AsyncEventingBasicConsumer(helper.Channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var evt = JsonSerializer.Deserialize<SensorReadingReceivedEvent>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (evt is null)
                {
                    _logger.LogWarning("Mensagem inválida (null deserialize).");
                    helper.Channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                using var scope = _provider.CreateScope();
                var engine = scope.ServiceProvider.GetRequiredService<AlertEngineService>();

                await engine.ProcessAsync(evt, stoppingToken);

                helper.Channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro processando mensagem.");

                //não requeue pra não travar em loop infinito
                helper.Channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        helper.Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("SensorReadingConsumer iniciado. Queue={Queue}", queueName);

        // mantém vivo até cancelamento
        stoppingToken.Register(() =>
        {
            _logger.LogInformation("Parando consumer...");
            helper.Dispose();
        });

        return Task.CompletedTask;
    }
}
