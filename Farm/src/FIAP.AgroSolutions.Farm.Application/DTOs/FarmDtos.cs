namespace FIAP.AgroSolutions.Farm.Application.DTOs;

public record CreateFarmRequest(string Name, string? LocationDescription);
public record FarmResponse(Guid Id, string Name, string? LocationDescription, DateTime CreatedAtUtc);

public record CreateFieldRequest(string Name, string Crop, string? BoundaryDescription);
public record FieldResponse(Guid Id, Guid FarmId, string Name, string Crop, string? BoundaryDescription, DateTime CreatedAtUtc);

public record FarmWithFieldsResponse(Guid Id, string Name, string? LocationDescription, DateTime CreatedAtUtc, List<FieldResponse> Fields);


