using FIAP.AgroSolutions.SensorIngestion.Domain.Entities;

namespace FIAP.AgroSolutions.SensorIngestion.Application.Abstractions;

public interface IReadingRepository
{
    Task AddAsync(SensorReading reading, CancellationToken ct);
    Task<List<SensorReading>> GetByFieldAsync(Guid fieldId, DateTime? fromUtc, DateTime? toUtc, int take, CancellationToken ct);
}
