using FIAP.AgroSolutions.Alerts.Domain.Entities;

namespace FIAP.AgroSolutions.Alerts.Application.Abstractions;

public interface IReadingsQuery
{
    Task AddAsync(SensorReading reading, CancellationToken ct);
    Task<decimal> SumRainAsync(Guid fieldId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
    Task<int> CountReadingsAsync(Guid fieldId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);

}
