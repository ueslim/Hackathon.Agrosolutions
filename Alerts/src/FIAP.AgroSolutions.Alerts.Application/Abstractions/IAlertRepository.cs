using FIAP.AgroSolutions.Alerts.Domain.Entities;

namespace FIAP.AgroSolutions.Alerts.Application.Abstractions;

public interface IAlertRepository
{
    Task AddAsync(Alert alert, CancellationToken ct);
    Task<List<Alert>> GetByFieldAsync(Guid fieldId, CancellationToken ct);
    Task<List<Alert>> GetActiveByFieldAsync(Guid fieldId, CancellationToken ct);
    Task<Alert?> GetByIdAsync(Guid alertId, CancellationToken ct);
}
