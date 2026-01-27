namespace FIAP.AgroSolutions.Farm.Domain.Entities;

public class Farm
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LocationDescription { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Field> Fields { get; set; } = new();
}
