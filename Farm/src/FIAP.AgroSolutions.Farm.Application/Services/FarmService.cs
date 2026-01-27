using FIAP.AgroSolutions.Farm.Application.Abstractions;
using FIAP.AgroSolutions.Farm.Application.DTOs;
using FIAP.AgroSolutions.Farm.Domain.Entities;

namespace FIAP.AgroSolutions.Farm.Application.Services;

public class FarmService : IFarmService
{
    private readonly IFarmRepository _farms;
    private readonly IFieldRepository _fields;

    public FarmService(IFarmRepository farms, IFieldRepository fields)
    {
        _farms = farms;
        _fields = fields;
    }
    public async Task<List<FarmWithFieldsResponse>> GetAllFarmsWithFieldsAsync(CancellationToken ct)
    {
        var farms = await _farms.GetAllWithFieldsAsync(ct);

        return farms
            .OrderBy(f => f.Name)
            .Select(f => new FarmWithFieldsResponse(
                f.Id,
                f.Name,
                f.LocationDescription,
                f.CreatedAtUtc,
                f.Fields
                    .OrderBy(x => x.Name)
                    .Select(x => new FieldResponse(
                        x.Id, x.FarmId, x.Name, x.Crop, x.BoundaryDescription, x.CreatedAtUtc
                    ))
                    .ToList()
            ))
            .ToList();
    }

    public async Task<FarmResponse> CreateFarmAsync(Guid userId, CreateFarmRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            throw new ArgumentException("Name is required");
        }

        var farm = new Domain.Entities.Farm
        {
            OwnerUserId = userId,
            Name = req.Name.Trim(),
            LocationDescription = req.LocationDescription?.Trim()
        };

        await _farms.AddAsync(farm, ct);

        return new FarmResponse(farm.Id, farm.Name, farm.LocationDescription, farm.CreatedAtUtc);
    }

    public async Task<List<FarmResponse>> GetFarmsAsync(Guid userId, CancellationToken ct)
    {
        var farms = await _farms.GetAllAsync(userId, ct);
        return farms
            .OrderBy(f => f.Name)
            .Select(f => new FarmResponse(f.Id, f.Name, f.LocationDescription, f.CreatedAtUtc))
            .ToList();
    }

    public async Task<FieldResponse> CreateFieldAsync(Guid userId, Guid farmId, CreateFieldRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            throw new ArgumentException("Name is required");
        }

        if (string.IsNullOrWhiteSpace(req.Crop))
        {
            throw new ArgumentException("Crop is required");
        }

        var farm = await _farms.GetByIdAsync(farmId, userId, ct);
        if (farm is null)
        {
            throw new KeyNotFoundException("Farm not found");
        }

        var field = new Field
        {
            FarmId = farmId,
            OwnerUserId = userId,
            Name = req.Name.Trim(),
            Crop = req.Crop.Trim(),
            BoundaryDescription = req.BoundaryDescription?.Trim()
        };

        await _fields.AddAsync(field, ct);

        return new FieldResponse(field.Id, field.FarmId, field.Name, field.Crop, field.BoundaryDescription, field.CreatedAtUtc);
    }

    public async Task<List<FieldResponse>> GetFieldsAsync(Guid userId, Guid? farmId, CancellationToken ct)
    {
        var fields = await _fields.GetAllAsync(userId, farmId, ct);
        return fields
            .OrderBy(f => f.Name)
            .Select(f => new FieldResponse(f.Id, f.FarmId, f.Name, f.Crop, f.BoundaryDescription, f.CreatedAtUtc))
            .ToList();
    }

    public async Task<FieldResponse?> GetFieldByIdAsync(Guid userId, Guid fieldId, CancellationToken ct)
    {
        var f = await _fields.GetByIdAsync(fieldId, userId, ct);
        return f is null
            ? null
            : new FieldResponse(f.Id, f.FarmId, f.Name, f.Crop, f.BoundaryDescription, f.CreatedAtUtc);
    }
}
