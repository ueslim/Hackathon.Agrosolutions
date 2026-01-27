namespace FIAP.AgroSolutions.Farm.Domain.Entities;

public class Field
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FarmId { get; set; }
    public Farm? Farm { get; set; }

    public Guid OwnerUserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Crop { get; set; } = string.Empty;
    public string? BoundaryDescription { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
