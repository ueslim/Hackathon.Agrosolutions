using FIAP.AgroSolutions.SensorIngestion.Application.Abstractions;
using FIAP.AgroSolutions.SensorIngestion.Domain.Entities;
using FIAP.AgroSolutions.SensorIngestion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.SensorIngestion.Infrastructure.Repositories;

public class ReadingRepository : IReadingRepository
{
    private readonly IngestionDbContext _db;

    public ReadingRepository(IngestionDbContext db) => _db = db;

    public async Task AddAsync(SensorReading reading, CancellationToken ct)
    {
        await _db.SensorReadings.AddAsync(reading, ct);
        await _db.SaveChangesAsync(ct); // salva leitura + outbox (se estiver no mesmo DbContext)
    }

    public async Task<List<SensorReading>> GetByFieldAsync(Guid fieldId, DateTime? fromUtc, DateTime? toUtc, int take, CancellationToken ct)
    {
        var q = _db.SensorReadings.AsNoTracking().Where(x => x.FieldId == fieldId);

        if (fromUtc.HasValue)
        {
            q = q.Where(x => x.MeasuredAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            q = q.Where(x => x.MeasuredAtUtc <= toUtc.Value);
        }

        return await q
            .OrderByDescending(x => x.MeasuredAtUtc)
            .Take(take)
            .ToListAsync(ct);
    }
}
