
namespace FIAP.AgroSolutions.Farm.Application.Abstractions;

public interface IFarmRepository
{
    Task<List<Domain.Entities.Farm>> GetAllWithFieldsAsync(Guid ownerUserId, CancellationToken ct);
    Task<Domain.Entities.Farm?> GetByIdAsync(Guid id, Guid ownerUserId, CancellationToken ct);
    Task<List<Domain.Entities.Farm>> GetAllAsync(Guid ownerUserId, CancellationToken ct);
    Task AddAsync(Domain.Entities.Farm farm, CancellationToken ct);
}
