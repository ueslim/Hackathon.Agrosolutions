using RabbitMQ.Client;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Messaging;

public class RabbitMQHelper : IDisposable
{
    private readonly IConnection _connection;
    private readonly RabbitMQ.Client.IModel _channel;

    public RabbitMQHelper(string hostName, int port, string userName, string password)
    {
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

    public RabbitMQ.Client.IModel Channel => _channel;

    public void EnsureQueueExists(string queueName)
    {
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Dispose()
    {
        try { _channel?.Close(); } catch { }
        try { _connection?.Close(); } catch { }
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
