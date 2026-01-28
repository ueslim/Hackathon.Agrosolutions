using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Domain.Enums;

namespace FIAP.AgroSolutions.Alerts.Application.Services;

public class SensorStaleService
{
    private readonly IAlertRepository _alerts;
    private readonly IAlertRuleRepository _rules;
    private readonly IFieldStateRepository _states;
    private readonly IUnitOfWork _uow;

    public SensorStaleService(
        IAlertRepository alerts,
        IAlertRuleRepository rules,
        IFieldStateRepository states,
        IUnitOfWork uow)
    {
        _alerts = alerts;
        _rules = rules;
        _states = states;
        _uow = uow;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var rules = await _rules.GetEnabledAsync(ct);
        var staleRule = rules.FirstOrDefault(r => r.Type == AlertType.SensorStale);

        if (staleRule is null || staleRule.DurationMinutes is null || staleRule.DurationMinutes <= 0)
        {
            return;
        }

        var threshold = TimeSpan.FromMinutes(staleRule.DurationMinutes.Value);
        var cooldown = TimeSpan.FromMinutes(staleRule.CooldownMinutes.GetValueOrDefault(60));

        var nowUtc = DateTime.UtcNow;
        var olderThanUtc = nowUtc - threshold;

        var staleFields = await _states.GetStaleAsync(olderThanUtc, ct);

        foreach (var fs in staleFields)
        {
            var active = await _alerts.GetActiveByFieldAsync(fs.FieldId, ct);
            if (active.Any(a => a.Type == AlertType.SensorStale))
            {
                continue;
            }

            var all = await _alerts.GetByFieldAsync(fs.FieldId, ct);
            var last = all
                .Where(a => a.Type == AlertType.SensorStale)
                .OrderByDescending(a => a.TriggeredAtUtc)
                .FirstOrDefault();

            if (last != null && (nowUtc - last.TriggeredAtUtc) < cooldown)
            {
                continue;
            }

            var msg = (staleRule.MessageTemplate ?? "")
                .Replace("{minutes}", staleRule.DurationMinutes.Value.ToString())
                .Replace("{measuredAt}", (fs.LastReadingAtUtc?.ToString("O") ?? "n/a"));

            await _alerts.AddAsync(new Alert
            {
                FieldId = fs.FieldId,
                Type = AlertType.SensorStale,
                Severity = staleRule.Severity,
                Message = msg,
                TriggeredAtUtc = nowUtc
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }
}
