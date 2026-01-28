using FIAP.AgroSolutions.Alerts.Domain.Enums;

namespace FIAP.AgroSolutions.Alerts.Domain.Entities;

public class AlertRule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string RuleKey { get; set; } = string.Empty;     // "DroughtV1"
    public string Name { get; set; } = string.Empty;        // "Alerta de Seca (24h)"

    public bool IsEnabled { get; set; } = true;

    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }

    public RuleKind Kind { get; set; }

    public SensorMetric Metric { get; set; }
    public ComparisonOp Operator { get; set; }
    public decimal ThresholdValue { get; set; }

    public int? DurationMinutes { get; set; }    // ThresholdDuration
    public int? CooldownMinutes { get; set; }    // ThresholdInstantCooldown

    public string MessageTemplate { get; set; } = string.Empty;

    public SensorMetric? SecondaryMetric { get; set; }
    public decimal? SecondaryMinValue { get; set; }
    public decimal? SecondaryMaxValue { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
