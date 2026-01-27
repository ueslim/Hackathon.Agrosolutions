using FIAP.AgroSolutions.SensorIngestion.Application.Abstractions;
using FIAP.AgroSolutions.SensorIngestion.Infrastructure.Persistence;

namespace FIAP.AgroSolutions.SensorIngestion.Infrastructure.Outbox;

public class OutboxWriter : IOutboxWriter
{
    private readonly IngestionDbContext _db;

    public OutboxWriter(IngestionDbContext db) => _db = db;

    public async Task EnqueueAsync(string type, string payload, DateTime occurredAtUtc, CancellationToken ct)
    {
        await _db.OutboxMessages.AddAsync(new OutboxMessage
        {
            Type = type,
            Payload = payload,
            OccurredAtUtc = occurredAtUtc
        }, ct);

        // Importante: NÃO salva aqui.
        // O SaveChanges acontece no mesmo request/transaction junto com a leitura.
    }
}
