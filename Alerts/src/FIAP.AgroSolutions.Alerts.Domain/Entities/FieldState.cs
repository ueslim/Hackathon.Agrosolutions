namespace FIAP.AgroSolutions.Alerts.Domain.Entities;

/// <summary>
/// Representa o estado atual (snapshot) de um campo agrícola.
/// 
/// Essa entidade mantém os últimos valores conhecidos das leituras
/// e serve para consultas rápidas, dashboards e avaliação eficiente
/// das regras, sem necessidade de percorrer todo o histórico.
/// </summary>
public class FieldState
{
    /// <summary>
    /// Identificador do campo agrícola.
    /// </summary>
    public Guid FieldId { get; set; }

    /// <summary>
    /// Momento da última leitura de sensor processada para este campo.
    /// </summary>
    public DateTime? LastReadingAtUtc { get; set; }

    /// <summary>
    /// Último valor conhecido de umidade do solo (%).
    /// </summary>
    public decimal? LastSoilMoisturePercent { get; set; }

    /// <summary>
    /// Último valor conhecido de temperatura (°C).
    /// </summary>
    public decimal? LastTemperatureC { get; set; }

    /// <summary>
    /// Último valor conhecido de chuva (mm).
    /// </summary>
    public decimal? LastRainMm { get; set; }

    /// <summary>
    /// Momento da última atualização do estado do campo.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Estados das regras associadas a este campo.
    /// Existe exatamente um RuleState por RuleKey.
    /// </summary>
    public List<RuleState> Rules { get; set; } = new();
}
