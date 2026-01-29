using FIAP.AgroSolutions.Alerts.Domain.Enums;

namespace FIAP.AgroSolutions.Alerts.Domain.Entities;

/// <summary>
/// Representa um alerta gerado pelo sistema quando uma regra agronômica
/// é atendida (por exemplo: seca, estresse térmico, excesso de chuva).
/// 
/// Um alerta é um registro histórico de um evento de risco detectado
/// para um determinado campo agrícola e permanece ativo até ser resolvido.
/// </summary>
public class Alert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FieldId { get; set; }

    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime TriggeredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
