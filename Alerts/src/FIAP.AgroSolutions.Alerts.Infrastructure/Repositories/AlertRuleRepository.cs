using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Repositories;

public class AlertRuleRepository : IAlertRuleRepository
{
    private readonly AlertsDbContext _db;

    public AlertRuleRepository(AlertsDbContext db) => _db = db;

    public async Task<List<AlertRule>> GetEnabledAsync(CancellationToken ct)
    {
        return await _db.AlertRules
            .AsNoTracking()
            .Where(r => r.IsEnabled)
            .OrderBy(r => r.RuleKey)
            .ToListAsync(ct);
    }
}
