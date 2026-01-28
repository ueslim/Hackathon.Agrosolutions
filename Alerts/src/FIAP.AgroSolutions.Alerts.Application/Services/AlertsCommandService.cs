using FIAP.AgroSolutions.Alerts.Application.Abstractions;

namespace FIAP.AgroSolutions.Alerts.Application.Services;

public class AlertsCommandService
{
    private readonly IAlertRepository _alerts;
    private readonly IUnitOfWork _uow;

    public AlertsCommandService(IAlertRepository alerts, IUnitOfWork uow)
    {
        _alerts = alerts;
        _uow = uow;
    }

    public async Task ResolveAsync(Guid alertId, CancellationToken ct)
    {
        var alert = await _alerts.GetByIdAsync(alertId, ct);

        if (alert is null)
        {
            throw new KeyNotFoundException("Alert not found");
        }

        if (alert.ResolvedAtUtc is not null)
        {
            return;
        }

        alert.ResolvedAtUtc = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }
}
