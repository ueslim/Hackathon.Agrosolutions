namespace FIAP.AgroSolutions.Alerts.Domain.Entities;

/// <summary>
/// Mantém o estado interno de uma regra para um campo agrícola específico.
/// 
/// Esta entidade funciona como a "memória" do motor de regras, permitindo
/// avaliar condições que dependem de tempo, evitar disparos duplicados
/// e controlar o ciclo de vida da regra.
/// 
/// Existe exatamente um RuleState para cada combinação de FieldId + RuleKey.
/// 
/// Exemplo:
/// "Se a umidade do solo ficar abaixo de 27% e permanecer assim por 24 horas,
/// então dispara um alerta de seca."
/// </summary>
public class RuleState
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador do campo agrícola ao qual este estado de regra pertence.
    /// </summary>
    public Guid FieldId { get; set; }

    /// <summary>
    /// Chave lógica da regra (ex: "DroughtV1").
    /// Utilizada para associar este estado à configuração da regra (AlertRule).
    /// </summary>
    public string RuleKey { get; set; } = string.Empty;

    /// <summary>
    /// Momento em que a regra entrou pela última vez na condição válida.
    /// 
    /// Usado principalmente em regras que exigem permanência no tempo
    /// (ex: ThresholdDuration).
    /// </summary>
    public DateTime? WindowStartUtc { get; set; }

    /// <summary>
    /// Momento do último disparo de alerta desta regra.
    /// 
    /// Utilizado para evitar disparos repetidos em curto intervalo
    /// (controle de cooldown).
    /// </summary>
    public DateTime? LastTriggeredAtUtc { get; set; }

    /// <summary>
    /// Indica se existe atualmente um alerta ativo associado a esta regra
    /// para o campo.
    /// </summary>
    public bool AlertActive { get; set; }

    /// <summary>
    /// Momento da última atualização significativa do estado da regra.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
