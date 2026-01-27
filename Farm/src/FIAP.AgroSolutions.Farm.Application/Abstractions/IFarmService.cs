using FIAP.AgroSolutions.Farm.Application.DTOs;

namespace FIAP.AgroSolutions.Farm.Application.Abstractions;

public interface IFarmService
{
    Task<List<FarmWithFieldsResponse>> GetAllFarmsWithFieldsAsync(CancellationToken ct);
    Task<FarmResponse> CreateFarmAsync(Guid userId, CreateFarmRequest req, CancellationToken ct);
    Task<List<FarmResponse>> GetFarmsAsync(Guid userId, CancellationToken ct);
    Task<FieldResponse> CreateFieldAsync(Guid userId, Guid farmId, CreateFieldRequest req, CancellationToken ct);
    Task<List<FieldResponse>> GetFieldsAsync(Guid userId, Guid? farmId, CancellationToken ct);
    Task<FieldResponse?> GetFieldByIdAsync(Guid userId, Guid fieldId, CancellationToken ct);
}
