using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Queries;

public class ReadingsQuery : IReadingsQuery
{
    private readonly AlertsDbContext _db;

    public ReadingsQuery(AlertsDbContext db)
    {
        _db = db;
    }
    public async Task AddAsync(SensorReading reading, CancellationToken ct)
    {
        var exists = await _db.SensorReadings.AnyAsync(x => x.Id == reading.Id, ct);

        if (exists)
        {
            return;
        }

        _db.SensorReadings.Add(reading);
    }

    public async Task<decimal> SumRainAsync(Guid fieldId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        fromUtc = EnsureUtc(fromUtc);
        toUtc = EnsureUtc(toUtc);

        var sum = await _db.SensorReadings
            .AsNoTracking()
            .Where(x => x.FieldId == fieldId &&
                        x.MeasuredAtUtc >= fromUtc &&
                        x.MeasuredAtUtc <= toUtc)
            .Select(x => (decimal?)x.RainMm)
            .SumAsync(ct);

        return sum ?? 0m;
    }

    public Task<int> CountReadingsAsync(Guid fieldId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        fromUtc = EnsureUtc(fromUtc);
        toUtc = EnsureUtc(toUtc);

        return _db.SensorReadings
            .AsNoTracking()
            .CountAsync(x => x.FieldId == fieldId &&
                             x.MeasuredAtUtc >= fromUtc &&
                             x.MeasuredAtUtc <= toUtc, ct);
    }

    private static DateTime EnsureUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
