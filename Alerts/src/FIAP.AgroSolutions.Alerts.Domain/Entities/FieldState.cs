namespace FIAP.AgroSolutions.Alerts.Domain.Entities;

public class FieldState
{
    public Guid FieldId { get; set; }

    public DateTime? LastReadingAtUtc { get; set; }
    public decimal? LastSoilMoisturePercent { get; set; }
    public decimal? LastTemperatureC { get; set; }
    public decimal? LastRainMm { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<RuleState> Rules { get; set; } = new();
}
