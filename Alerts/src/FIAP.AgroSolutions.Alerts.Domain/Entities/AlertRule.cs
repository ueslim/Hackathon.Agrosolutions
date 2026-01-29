using FIAP.AgroSolutions.Alerts.Domain.Enums;

namespace FIAP.AgroSolutions.Alerts.Domain.Entities;

/// <summary>
/// Define uma regra agronômica configurável utilizada pelo motor de alertas.
/// 
/// Uma regra descreve:
/// - qual métrica observar (ex: umidade, temperatura)
/// - qual condição deve ser atendida
/// - por quanto tempo essa condição deve persistir
/// - e como o alerta deve ser apresentado ao usuário
/// 
/// As regras são dados (configuração) e não código,
/// permitindo evolução do sistema sem recompilação.
/// </summary>
public class AlertRule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador lógico da regra.
    /// Usado para associar o estado da regra (RuleState) a um campo.
    /// Ex: "DroughtV1".
    /// </summary>
    public string RuleKey { get; set; } = string.Empty;

    /// <summary>
    /// Nome descritivo da regra, utilizado para leitura humana.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indica se a regra está ativa e deve ser avaliada pelo motor.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Tipo do alerta gerado por esta regra (ex: Drought, HeatStress).
    /// </summary>
    public AlertType Type { get; set; }

    /// <summary>
    /// Severidade do alerta quando a regra é disparada.
    /// </summary>
    public AlertSeverity Severity { get; set; }

    /// <summary>
    /// Define o comportamento da regra (duração, instantânea, janela, etc).
    /// </summary>
    public RuleKind Kind { get; set; }

    /// <summary>
    /// Métrica principal avaliada pela regra.
    /// Ex: umidade do solo, temperatura, chuva.
    /// </summary>
    public SensorMetric Metric { get; set; }

    /// <summary>
    /// Operador de comparação utilizado na avaliação da métrica.
    /// </summary>
    public ComparisonOp Operator { get; set; }

    /// <summary>
    /// Valor limite utilizado na comparação da métrica.
    /// </summary>
    public decimal ThresholdValue { get; set; }

    /// <summary>
    /// Tempo mínimo (em minutos) que a condição deve permanecer verdadeira
    /// para que o alerta seja disparado.
    /// Utilizado em regras do tipo ThresholdDuration e similares.
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Intervalo mínimo (em minutos) entre disparos consecutivos da mesma regra,
    /// evitando geração excessiva de alertas.
    /// </summary>
    public int? CooldownMinutes { get; set; }

    /// <summary>
    /// Template da mensagem apresentada ao usuário quando o alerta é disparado.
    /// Pode conter placeholders como {threshold}, {value}, {minutes}.
    /// </summary>
    public string MessageTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Métrica secundária utilizada em regras compostas,
    /// como risco de doença (ex: temperatura + umidade).
    /// </summary>
    public SensorMetric? SecondaryMetric { get; set; }

    /// <summary>
    /// Valor mínimo aceitável para a métrica secundária.
    /// </summary>
    public decimal? SecondaryMinValue { get; set; }

    /// <summary>
    /// Valor máximo aceitável para a métrica secundária.
    /// </summary>
    public decimal? SecondaryMaxValue { get; set; }

    /// <summary>
    /// Data de criação da regra (auditoria).
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização da regra.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }
}
