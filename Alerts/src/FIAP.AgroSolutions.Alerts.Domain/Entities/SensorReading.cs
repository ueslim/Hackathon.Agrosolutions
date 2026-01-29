namespace FIAP.AgroSolutions.Alerts.Domain.Entities;

/// <summary>
/// Representa uma leitura de sensor recebida pelo sistema,
/// contendo as medições ambientais coletadas em um campo agrícola.
/// 
/// As leituras são armazenadas como histórico e utilizadas
/// pelo motor de regras para avaliação de condições, janelas
/// de tempo e agregações.
/// </summary>
public class SensorReading
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FieldId { get; set; }

    public decimal SoilMoisturePercent { get; set; }

    public decimal TemperatureC { get; set; }

    public decimal RainMm { get; set; }

    public DateTime MeasuredAtUtc { get; set; }

    public DateTime ReceivedAtUtc { get; set; }
}
