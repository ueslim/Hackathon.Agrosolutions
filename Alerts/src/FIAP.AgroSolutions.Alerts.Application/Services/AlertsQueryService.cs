using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Application.DTOs;

namespace FIAP.AgroSolutions.Alerts.Application.Services;

public class AlertsQueryService
{
    private readonly IAlertRepository _alerts;
    private readonly IFieldStateRepository _states;

    public AlertsQueryService(IAlertRepository alerts, IFieldStateRepository states)
    {
        _alerts = alerts;
        _states = states;
    }

    public async Task<List<AlertResponse>> GetAlertsByFieldAsync(Guid fieldId, CancellationToken ct)
    {
        var list = await _alerts.GetByFieldAsync(fieldId, ct);

        return list
            .OrderByDescending(a => a.TriggeredAtUtc)
            .Select(a => new AlertResponse(
                a.Id,
                a.FieldId,
                a.Type.ToString(),
                a.Severity.ToString(),
                a.Message,
                a.TriggeredAtUtc,
                a.ResolvedAtUtc
            ))
            .ToList();
    }

    public async Task<FieldStatusResponse> GetFieldStatusAsync(Guid fieldId, CancellationToken ct)
    {
        var state = await _states.GetAsync(fieldId, ct);
        var activeAlerts = await _alerts.GetActiveByFieldAsync(fieldId, ct);

        var activeItems = activeAlerts
            .OrderByDescending(a => a.TriggeredAtUtc)
            .Select(a => new ActiveAlertItem(
                a.Id,
                a.Type.ToString(),
                a.Severity.ToString(),
                a.Message,
                a.TriggeredAtUtc
            ))
            .ToList();

        var status = activeAlerts.Count == 0
            ? "Normal"
            : ComputeOverallStatus(activeAlerts);

        return new FieldStatusResponse(
            fieldId,
            status,
            state?.LastSoilMoisturePercent,
            state?.LastReadingAtUtc,
            state?.UpdatedAtUtc ?? DateTime.UtcNow,
            activeItems
        );
    }

    private static string ComputeOverallStatus(List<Domain.Entities.Alert> activeAlerts)
    {
        var maxSeverity = activeAlerts.Max(a => a.Severity);

        return maxSeverity switch
        {
            AlertSeverity.Critical => "Crítico",
            AlertSeverity.Warning => "Atenção",
            AlertSeverity.Info => "Alerta",
            _ => "Alerta"
        };
    }

}
