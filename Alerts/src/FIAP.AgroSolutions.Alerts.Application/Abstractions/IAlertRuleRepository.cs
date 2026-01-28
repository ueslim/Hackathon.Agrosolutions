using FIAP.AgroSolutions.Alerts.Domain.Entities;

namespace FIAP.AgroSolutions.Alerts.Application.Abstractions;

public interface IAlertRuleRepository
{
    Task<List<AlertRule>> GetEnabledAsync(CancellationToken ct);
}
