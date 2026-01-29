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

/// <summary>
/// Consumer responsável por escutar a fila de leituras de sensores (RabbitMQ)
/// e acionar o motor de alertas sempre que uma nova leitura chega.
/// 
/// Ele NÃO contém regra de negócio.
/// Apenas recebe mensagens, desserializa e delega para o AlertEngineService.
/// </summary>
public class SensorReadingConsumer : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<SensorReadingConsumer> _logger;
    private readonly IConfiguration _config;

    public SensorReadingConsumer(
        IServiceProvider provider,
        ILogger<SensorReadingConsumer> logger,
        IConfiguration config)
    {
        _provider = provider;
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Método principal do BackgroundService.
    /// 
    /// Ele configura a conexão com o RabbitMQ,
    /// registra o consumer e mantém o processo vivo.
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbit = _config.GetSection("RabbitMQ");
        var queues = _config.GetSection("Queues");

        // Cria helper para conexão com RabbitMQ
        var helper = new RabbitMQHelper(
            hostName: rabbit["HostName"] ?? "localhost",
            port: int.TryParse(rabbit["Port"], out var p) ? p : 5672,
            userName: rabbit["UserName"] ?? "guest",
            password: rabbit["Password"] ?? "guest"
        );

        var queueName = queues["SensorReadings"] ?? "sensor.readings";
        helper.EnsureQueueExists(queueName);

        // QoS: limita a quantidade de mensagens processadas em paralelo
        helper.Channel.BasicQos(0, 10, false);

        var consumer = new AsyncEventingBasicConsumer(helper.Channel);

        // Executado sempre que uma mensagem chega na fila
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                // Desserializa o evento recebido
                var evt = JsonSerializer.Deserialize<SensorReadingReceivedEvent>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (evt is null)
                {
                    _logger.LogWarning("Mensagem inválida (deserialize retornou null).");
                    helper.Channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                // Cria escopo para resolver dependências corretamente
                using var scope = _provider.CreateScope();
                var engine = scope.ServiceProvider.GetRequiredService<AlertEngineService>();

                // Delegação total da regra de negócio
                await engine.ProcessAsync(evt, stoppingToken);

                // Confirma processamento da mensagem
                helper.Channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro processando mensagem de leitura.");

                // NACK sem requeue para evitar loop infinito
                helper.Channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };

        helper.Channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );

        _logger.LogInformation(
            "SensorReadingConsumer iniciado. Escutando fila {Queue}",
            queueName
        );

        // Finaliza corretamente ao parar o serviço
        stoppingToken.Register(() =>
        {
            _logger.LogInformation("Parando consumer de leituras...");
            helper.Dispose();
        });

        return Task.CompletedTask;
    }
}
