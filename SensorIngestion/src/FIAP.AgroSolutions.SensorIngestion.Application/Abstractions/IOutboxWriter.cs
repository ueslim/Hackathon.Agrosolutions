namespace FIAP.AgroSolutions.SensorIngestion.Application.Abstractions;

public interface IOutboxWriter
{
    Task EnqueueAsync(string type, string payload, DateTime occurredAtUtc, CancellationToken ct);
}
