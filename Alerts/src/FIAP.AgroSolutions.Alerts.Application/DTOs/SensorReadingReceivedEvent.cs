namespace FIAP.AgroSolutions.Alerts.Application.DTOs;

public class SensorReadingReceivedEvent
{
    public string EventType { get; set; } = default!;
    public Guid ReadingId { get; set; }
    public Guid FieldId { get; set; }
    public decimal SoilMoisturePercent { get; set; }
    public decimal TemperatureC { get; set; }
    public decimal RainMm { get; set; }
    public DateTime MeasuredAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
}
