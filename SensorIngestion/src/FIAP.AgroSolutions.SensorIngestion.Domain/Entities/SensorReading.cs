namespace FIAP.AgroSolutions.SensorIngestion.Domain.Entities;

/// <summary>
/// Representa uma leitura bruta de um sensor em um determinado momento.
/// 
/// Esta entidade guarda o histórico completo de medições recebidas,
/// exatamente como vieram do sensor (ou do simulador).
///
/// Importante:
/// - Não contém lógica de alerta.
/// - Serve como base para cálculos, agregações e regras.
/// - É imutável conceitualmente (uma leitura passada nunca muda).
///
/// Exemplo:
/// "No dia 28/01 às 18:42, o sensor mediu:
///  - Umidade do solo: 27.9%
///  - Temperatura: 36°C
///  - Chuva: 0.1mm"
/// </summary>
public class SensorReading
{
    /// <summary>
    /// Identificador único da leitura.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador do talhão (Field) onde o sensor está instalado.
    /// </summary>
    public Guid FieldId { get; set; }

    /// <summary>
    /// Umidade do solo em porcentagem (0 a 100).
    /// </summary>
    public decimal SoilMoisturePercent { get; set; }

    /// <summary>
    /// Temperatura medida em graus Celsius.
    /// </summary>
    public decimal TemperatureC { get; set; }

    /// <summary>
    /// Quantidade de chuva medida em milímetros (mm).
    /// </summary>
    public decimal RainMm { get; set; }

    /// <summary>
    /// Momento exato em que o sensor realizou a medição (UTC).
    /// </summary>
    public DateTime MeasuredAtUtc { get; set; }

    /// <summary>
    /// Momento em que o sistema recebeu a leitura (UTC).
    /// 
    /// Pode ser diferente de MeasuredAtUtc em cenários reais
    /// (ex: sensor offline que envia dados atrasados).
    /// </summary>
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
}
