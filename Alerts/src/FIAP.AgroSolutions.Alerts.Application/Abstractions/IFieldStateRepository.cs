using FIAP.AgroSolutions.Alerts.Domain.Entities;

namespace FIAP.AgroSolutions.Alerts.Application.Abstractions;

public interface IFieldStateRepository
{
    Task<FieldState?> GetAsync(Guid fieldId, CancellationToken ct);
    Task UpsertAsync(FieldState state, CancellationToken ct);
}
