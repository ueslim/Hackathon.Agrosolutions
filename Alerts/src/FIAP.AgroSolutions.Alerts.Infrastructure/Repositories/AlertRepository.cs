using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Domain.Enums;
using FIAP.AgroSolutions.Alerts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AlertsDbContext _db;

    public AlertRepository(AlertsDbContext db) => _db = db;

    public async Task AddAsync(Alert alert, CancellationToken ct)
    {
        _db.Alerts.Add(alert);
    }

    public async Task<List<Alert>> GetByFieldAsync(Guid fieldId, CancellationToken ct)
    {
        return await _db.Alerts
            .AsNoTracking()
            .Where(a => a.FieldId == fieldId)
            .OrderByDescending(a => a.TriggeredAtUtc)
            .ToListAsync(ct);
    }

    public async Task<List<Alert>> GetActiveByFieldAsync(Guid fieldId, CancellationToken ct)
    {
        return await _db.Alerts
            .AsNoTracking()
            .Where(a => a.FieldId == fieldId && a.ResolvedAtUtc == null)
            .OrderByDescending(a => a.TriggeredAtUtc)
            .ToListAsync(ct);
    }

    public Task<Alert?> GetByIdAsync(Guid alertId, CancellationToken ct)
    {
        return _db.Alerts.FirstOrDefaultAsync(a => a.Id == alertId, ct);
    }
    public Task<bool> HasActiveAsync(Guid fieldId, AlertType type, CancellationToken ct)
    {
        return _db.Alerts.AsNoTracking()
            .AnyAsync(a => a.FieldId == fieldId && a.Type == type && a.ResolvedAtUtc == null, ct);
    }
    public Task<Alert?> GetActiveByFieldAndTypeAsync(Guid fieldId, AlertType type, CancellationToken ct)
    {
        return _db.Alerts
            .FirstOrDefaultAsync(a => a.FieldId == fieldId
                                   && a.Type == type
                                   && a.ResolvedAtUtc == null, ct);
    }

    public async Task<int> ResolveAsync(Guid alertId, DateTime resolvedAtUtc, CancellationToken ct)
    {
        resolvedAtUtc = EnsureUtc(resolvedAtUtc);

        return await _db.Alerts
            .Where(a => a.Id == alertId && a.ResolvedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.ResolvedAtUtc, resolvedAtUtc),
                ct);
    }

    public async Task<int> ResolveActiveByTypeAsync(Guid fieldId, AlertType type, DateTime resolvedAtUtc, CancellationToken ct)
    {
        resolvedAtUtc = EnsureUtc(resolvedAtUtc);

        return await _db.Alerts
            .Where(a => a.FieldId == fieldId && a.Type == type && a.ResolvedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.ResolvedAtUtc, resolvedAtUtc),
                ct);
    }

    public async Task<int> ResolveAllActiveAsync(Guid fieldId, DateTime resolvedAtUtc, CancellationToken ct)
    {
        resolvedAtUtc = EnsureUtc(resolvedAtUtc);

        return await _db.Alerts
            .Where(a => a.FieldId == fieldId && a.ResolvedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.ResolvedAtUtc, resolvedAtUtc),
                ct);
    }

    private static DateTime EnsureUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

}
