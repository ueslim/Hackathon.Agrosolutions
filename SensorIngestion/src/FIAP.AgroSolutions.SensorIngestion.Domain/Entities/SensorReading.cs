namespace FIAP.AgroSolutions.SensorIngestion.Domain.Entities;

public class SensorReading
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FieldId { get; set; }

    public decimal SoilMoisturePercent { get; set; }
    public decimal TemperatureC { get; set; }
    public decimal RainMm { get; set; }

    public DateTime MeasuredAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
}
