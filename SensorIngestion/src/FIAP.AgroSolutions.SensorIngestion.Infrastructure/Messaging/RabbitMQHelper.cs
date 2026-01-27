using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace FIAP.AgroSolutions.SensorIngestion.Infrastructure.Messaging;

public class RabbitMQHelper : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQHelper> _logger;

    public RabbitMQHelper(string hostName, int port, string userName, string password, ILogger<RabbitMQHelper> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void EnsureQueueExists(string queueName)
    {
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Publish(string queueName, string message)
    {
        EnsureQueueExists(queueName);

        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }

    public void Dispose()
    {
        try { _channel?.Close(); } catch { }
        try { _connection?.Close(); } catch { }
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
