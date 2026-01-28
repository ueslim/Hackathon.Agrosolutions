using FIAP.AgroSolutions.Alerts.Domain.Entities;

namespace FIAP.AgroSolutions.Alerts.Application.Abstractions;

public interface IFieldStateRepository
{
    Task<List<FieldState>> GetStaleAsync(DateTime olderThanUtc, CancellationToken ct);
    Task<FieldState?> GetAsync(Guid fieldId, CancellationToken ct);
    Task UpsertAsync(FieldState state, CancellationToken ct);
}
