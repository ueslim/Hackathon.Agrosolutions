namespace FIAP.AgroSolutions.Alerts.Domain.Enums;

/// <summary>
/// Define o tipo de comportamento de avaliação de uma regra de alerta.
/// 
/// Cada valor representa uma estratégia diferente de interpretação
/// das leituras de sensores ao longo do tempo.
/// </summary>
public enum RuleKind
{
    /// <summary>
    /// A condição da regra precisa permanecer verdadeira por um
    /// período contínuo de tempo para que o alerta seja disparado.
    /// 
    /// Exemplo: umidade do solo abaixo do limite por 24 horas.
    /// </summary>
    ThresholdDuration = 1,

    /// <summary>
    /// O alerta é disparado imediatamente quando a condição é atendida,
    /// mas respeita um intervalo mínimo (cooldown) entre disparos.
    /// 
    /// Exemplo: chuva intensa detectada.
    /// </summary>
    ThresholdInstantCooldown = 2,

    /// <summary>
    /// A regra avalia a soma acumulada de uma métrica dentro de
    /// uma janela de tempo.
    /// 
    /// Exemplo: soma de chuva dos últimos 7 dias abaixo do mínimo esperado.
    /// </summary>
    WindowSumThreshold = 3,

    /// <summary>
    /// A regra exige que duas métricas atendam simultaneamente
    /// a uma condição por um período contínuo de tempo.
    /// 
    /// Exemplo: alta umidade combinada com temperatura favorável,
    /// indicando risco de doença.
    /// </summary>
    DualMetricDuration = 4
}
