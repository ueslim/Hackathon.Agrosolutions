using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Domain.Enums;

namespace FIAP.AgroSolutions.Alerts.Application.Abstractions;

public interface IAlertRepository
{
    Task<List<Alert>> GetByFieldAsync(Guid fieldId, CancellationToken ct);
    Task<List<Alert>> GetActiveByFieldAsync(Guid fieldId, CancellationToken ct);
    Task<Alert?> GetByIdAsync(Guid alertId, CancellationToken ct);
    Task<Alert?> GetActiveByFieldAndTypeAsync(Guid fieldId, AlertType type, CancellationToken ct);
    Task<bool> HasActiveAsync(Guid fieldId, AlertType type, CancellationToken ct);
    Task AddAsync(Alert alert, CancellationToken ct);
    Task<int> ResolveAsync(Guid alertId, DateTime resolvedAtUtc, CancellationToken ct);
    Task<int> ResolveActiveByTypeAsync(Guid fieldId, AlertType type, DateTime resolvedAtUtc, CancellationToken ct);
    Task<int> ResolveAllActiveAsync(Guid fieldId, DateTime resolvedAtUtc, CancellationToken ct);
}
