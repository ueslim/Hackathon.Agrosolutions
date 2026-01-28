namespace FIAP.AgroSolutions.Alerts.Domain.Entities;

public class RuleState
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FieldId { get; set; }
    public string RuleKey { get; set; } = string.Empty;

    // ThresholdDuration: início da janela
    public DateTime? WindowStartUtc { get; set; }

    // Cooldown: último disparo (evita spam)
    public DateTime? LastTriggeredAtUtc { get; set; }

    public bool AlertActive { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
