namespace FIAP.AgroSolutions.Alerts.Domain.Enums;

/// <summary>
/// Define as métricas ambientais coletadas pelos sensores
/// e utilizadas pelo motor de regras para avaliação de alertas.
/// </summary>
public enum SensorMetric
{
    /// <summary>
    /// Percentual de umidade do solo.
    /// </summary>
    SoilMoisturePercent = 1,

    /// <summary>
    /// Temperatura ambiente medida em graus Celsius.
    /// </summary>
    TemperatureC = 2,

    /// <summary>
    /// Quantidade de chuva medida em milímetros.
    /// </summary>
    RainMm = 3
}
