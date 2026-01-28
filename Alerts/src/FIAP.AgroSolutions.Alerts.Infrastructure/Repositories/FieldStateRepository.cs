using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Repositories;

public class FieldStateRepository : IFieldStateRepository
{
    private readonly AlertsDbContext _db;

    public FieldStateRepository(AlertsDbContext db) => _db = db;

    public async Task<List<FieldState>> GetStaleAsync(DateTime olderThanUtc, CancellationToken ct)
    {
        olderThanUtc = olderThanUtc.Kind == DateTimeKind.Utc
            ? olderThanUtc
            : DateTime.SpecifyKind(olderThanUtc, DateTimeKind.Utc);

        return await _db.FieldStates
            .AsNoTracking()
            .Where(x => x.LastReadingAtUtc != null && x.LastReadingAtUtc < olderThanUtc)
            .ToListAsync(ct);
    }

    public async Task<FieldState?> GetAsync(Guid fieldId, CancellationToken ct)
    {
        return await _db.FieldStates
        .Include(x => x.Rules)
        .FirstOrDefaultAsync(x => x.FieldId == fieldId, ct);
    }

    public async Task UpsertAsync(FieldState state, CancellationToken ct)
    {
        var existing = await _db.FieldStates
            .Include(x => x.Rules)
            .FirstOrDefaultAsync(x => x.FieldId == state.FieldId, ct);

        if (existing is null)
        {
            _db.FieldStates.Add(state);
            return;
        }

        _db.Entry(existing).CurrentValues.SetValues(state);

        foreach (var rs in state.Rules)
        {
            var current = existing.Rules.FirstOrDefault(x => x.RuleKey == rs.RuleKey);
            if (current is null)
            {
                existing.Rules.Add(rs);
            }
            else
            {
                _db.Entry(current).CurrentValues.SetValues(rs);
            }
        }
    }
}
