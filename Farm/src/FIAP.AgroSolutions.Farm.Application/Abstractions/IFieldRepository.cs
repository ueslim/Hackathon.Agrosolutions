using FIAP.AgroSolutions.Farm.Domain.Entities;

namespace FIAP.AgroSolutions.Farm.Application.Abstractions;

public interface IFieldRepository
{
    Task<Field?> GetByIdAsync(Guid id, Guid ownerUserId, CancellationToken ct);
    Task<List<Field>> GetAllAsync(Guid ownerUserId, Guid? farmId, CancellationToken ct);
    Task AddAsync(Field field, CancellationToken ct);
}
