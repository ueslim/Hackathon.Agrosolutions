using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Application.DTOs;
using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Domain.Enums;

namespace FIAP.AgroSolutions.Alerts.Application.Services;

/// <summary>
/// Motor de alertas.
/// 
/// Pense assim:
/// 1) Chega uma leitura do sensor (umidade/temperatura/chuva)
/// 2) Salvamos a leitura
/// 3) Atualizamos um "resumo rápido" do talhão (FieldState)
/// 4) Rodamos todas as regras habilitadas (AlertRule)
/// 5) Se uma regra ficou verdadeira por tempo suficiente, criamos um Alert
/// 
/// Importante: o motor roda apenas quando chega leitura (evento).
/// </summary>
public class AlertEngineService
{
    private readonly IFieldStateRepository _states;
    private readonly IAlertRepository _alerts;
    private readonly IAlertRuleRepository _rules;
    private readonly IReadingsQuery _readings;
    private readonly IUnitOfWork _uow;

    public AlertEngineService(IFieldStateRepository states, IAlertRepository alerts, IAlertRuleRepository rules, IReadingsQuery readings, IUnitOfWork uow)
    {
        _states = states;
        _alerts = alerts;
        _rules = rules;
        _readings = readings;
        _uow = uow;
    }

    /// <summary>
    /// Entrada principal do motor: processa um evento de leitura recebida.
    /// </summary>
    public async Task ProcessAsync(SensorReadingReceivedEvent evt, CancellationToken ct)
    {
        // Normaliza datas pra UTC (evita bugs de fuso)
        var measuredAtUtc = EnsureUtc(evt.MeasuredAtUtc);
        var receivedAtUtc = EnsureUtc(evt.ReceivedAtUtc);

        // 1) Guarda a leitura (histórico para gráficos e regras por janela/soma)
        await _readings.AddAsync(new SensorReading
        {
            Id = evt.ReadingId,
            FieldId = evt.FieldId,
            SoilMoisturePercent = evt.SoilMoisturePercent,
            TemperatureC = evt.TemperatureC,
            RainMm = evt.RainMm,
            MeasuredAtUtc = measuredAtUtc,
            ReceivedAtUtc = receivedAtUtc
        }, ct);

        // 2) Busca (ou cria) o resumo rápido do talhão
        // FieldState é tipo um "cache no banco" do último estado.
        var state = await _states.GetAsync(evt.FieldId, ct)
            ?? new FieldState { FieldId = evt.FieldId };

        state.Rules ??= new List<RuleState>();

        // Atualiza os "últimos valores conhecidos" do talhão
        state.LastReadingAtUtc = measuredAtUtc;
        state.LastSoilMoisturePercent = evt.SoilMoisturePercent;
        state.LastTemperatureC = evt.TemperatureC;
        state.LastRainMm = evt.RainMm;
        state.UpdatedAtUtc = receivedAtUtc;

        // 3) Carrega regras habilitadas e aplica uma por uma
        var rules = await _rules.GetEnabledAsync(ct);

        foreach (var rule in rules)
        {
            await ApplyRuleAsync(rule, state, evt, measuredAtUtc, ct);
        }

        // 4) Persiste o resumo do talhão e estados das regras
        await _states.UpsertAsync(state, ct);
        await _uow.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Decide qual "tipo de regra" aplicar.
    /// </summary>
    private async Task ApplyRuleAsync(AlertRule rule, FieldState state, SensorReadingReceivedEvent evt, DateTime measuredAtUtc, CancellationToken ct)
    {
        // RuleState é a "memória" da regra por talhão.
        // Ex: para um talhão X e uma regra "DroughtV1", guardamos
        // quando começou a ficar seco e quando disparou pela última vez.
        var ruleState = GetOrCreateRuleState(state, rule.RuleKey);

        // Pega o valor da métrica que a regra quer (umidade / temp / chuva)
        var metricValue = GetMetricValue(rule.Metric, evt);

        // Verifica a condição simples: exemplo "umidade < 30"
        var isInCondition = Compare(metricValue, rule.Operator, rule.ThresholdValue);

        // Cada Kind tem uma lógica diferente de disparo
        switch (rule.Kind)
        {
            // "Precisa ficar assim por X minutos"
            case RuleKind.ThresholdDuration:
                await ApplyThresholdDurationAsync(rule, ruleState, state.FieldId, isInCondition, measuredAtUtc, ct);
                break;

            // "Dispara imediatamente, mas com cooldown pra não spammar"
            case RuleKind.ThresholdInstantCooldown:
                await ApplyInstantCooldownAsync(rule, ruleState, state.FieldId, isInCondition, measuredAtUtc, metricValue, ct);
                break;

            // "Olha uma janela (ex: últimos 10 min) e soma/conta (ex: soma chuva)"
            case RuleKind.WindowSumThreshold:
                await ApplyWindowSumThresholdAsync(rule, ruleState, state.FieldId, measuredAtUtc, ct);
                break;

            // "Duas condições ao mesmo tempo por X minutos"
            // Ex: umidade alta E temperatura 20..32 por X min
            case RuleKind.DualMetricDuration:
                await ApplyDualMetricDurationAsync(rule, ruleState, state.FieldId, evt, measuredAtUtc, ct);
                break;
        }
    }

    /// <summary>
    /// Regra do tipo: "se ficar na condição por X minutos, dispara 1 alerta".
    /// 
    /// Exemplo (frase humana):
    /// "Se a umidade ficar abaixo de 30% e permanecer assim por 2 minutos, dispara Drought."
    /// </summary>
    private async Task ApplyThresholdDurationAsync(AlertRule rule, RuleState ruleState, Guid fieldId, bool isInCondition, DateTime measuredAtUtc, CancellationToken ct)
    {
        if (rule.DurationMinutes is null || rule.DurationMinutes <= 0)
        {
            return;
        }

        var window = TimeSpan.FromMinutes(rule.DurationMinutes.Value);

        // 1) Se NÃO está na condição, zera a contagem de tempo.
        // Ex: se a umidade voltou ao normal, então "para de contar".
        if (!isInCondition)
        {
            ruleState.WindowStartUtc = null;
            ruleState.AlertActive = false;
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        // 2) Se entrou na condição AGORA, marca o começo da janela.
        // Ex: "começou a ficar seco às 10:00"
        if (ruleState.WindowStartUtc is null)
        {
            ruleState.WindowStartUtc = measuredAtUtc;
            ruleState.AlertActive = false;
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        // 3) Já estava na condição: calcula há quanto tempo está assim.
        var duration = measuredAtUtc - ruleState.WindowStartUtc.Value;

        // Ainda não completou o tempo necessário? então não dispara.
        if (duration < window)
        {
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        // 4) Já completou o tempo: evita duplicar alerta do mesmo tipo.
        var hasActive = await _alerts.HasActiveAsync(fieldId, rule.Type, ct);
        if (hasActive)
        {
            ruleState.AlertActive = true;
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        // 5) Dispara o alerta (uma vez).
        var alert = new Alert
        {
            FieldId = fieldId,
            Type = rule.Type,
            Severity = rule.Severity,
            Message = RenderMessage(rule, measuredAtUtc),
            TriggeredAtUtc = measuredAtUtc
        };

        await _alerts.AddAsync(alert, ct);

        // Atualiza a "memória" da regra:
        ruleState.AlertActive = true;
        ruleState.LastTriggeredAtUtc = measuredAtUtc;

        // Recomeça a janela para permitir um próximo disparo futuro,
        // caso você resolva o alerta manualmente e continue na condição.
        ruleState.WindowStartUtc = measuredAtUtc;
        ruleState.UpdatedAtUtc = measuredAtUtc;
    }

    /// <summary>
    /// Regra do tipo: "bateu o limiar? dispara na hora, mas respeita cooldown".
    /// Ex: chuva >= 20mm -> HeavyRain (com cooldown).
    /// </summary>
    private async Task ApplyInstantCooldownAsync(AlertRule rule, RuleState ruleState, Guid fieldId, bool isInCondition, DateTime measuredAtUtc, decimal metricValue, CancellationToken ct)
    {
        // Se não está na condição, não faz nada (e marca como não ativo).
        if (!isInCondition)
        {
            ruleState.AlertActive = false;
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        // Evita duplicar se já existe um alerta ativo do mesmo tipo.
        var hasActive = await _alerts.HasActiveAsync(fieldId, rule.Type, ct);
        if (hasActive)
        {
            ruleState.AlertActive = true;
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        // Cooldown: "não dispara de novo antes de X minutos"
        var cooldown = TimeSpan.FromMinutes(rule.CooldownMinutes.GetValueOrDefault(0));
        var canFire =
            ruleState.LastTriggeredAtUtc is null ||
            cooldown == TimeSpan.Zero ||
            (measuredAtUtc - ruleState.LastTriggeredAtUtc.Value) >= cooldown;

        if (!canFire)
        {
            return;
        }

        // Exemplo: severidade dinâmica para chuva MUITO alta
        var severity = rule.Severity;
        if (rule.Type == AlertType.HeavyRain && metricValue >= 50m)
        {
            severity = AlertSeverity.Critical;
        }

        var alert = new Alert
        {
            FieldId = fieldId,
            Type = rule.Type,
            Severity = severity,
            Message = RenderMessage(rule, measuredAtUtc, metricValue),
            TriggeredAtUtc = measuredAtUtc
        };

        await _alerts.AddAsync(alert, ct);

        ruleState.AlertActive = true;
        ruleState.LastTriggeredAtUtc = measuredAtUtc;
        ruleState.UpdatedAtUtc = measuredAtUtc;
    }

    /// <summary>
    /// Regra por janela/soma:
    /// Ex: "se a soma de chuva nos últimos 10 minutos < 5mm, dispara NoRain".
    /// </summary>
    private async Task ApplyWindowSumThresholdAsync(AlertRule rule, RuleState ruleState, Guid fieldId, DateTime measuredAtUtc, CancellationToken ct)
    {
        if (rule.DurationMinutes is null || rule.DurationMinutes <= 0)
        {
            return;
        }

        var window = TimeSpan.FromMinutes(rule.DurationMinutes.Value);
        var fromUtc = measuredAtUtc - window;

        // Garante um mínimo de histórico antes de avaliar soma/contagem.
        var count = await _readings.CountReadingsAsync(fieldId, fromUtc, measuredAtUtc, ct);
        if (count < 10)
        {
            return;
        }

        // Soma da chuva na janela
        var sum = await _readings.SumRainAsync(fieldId, fromUtc, measuredAtUtc, ct);

        // Ex: sum < 5
        var isInCondition = Compare(sum, rule.Operator, rule.ThresholdValue);

        if (!isInCondition)
        {
            ruleState.AlertActive = false;
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        var hasActive = await _alerts.HasActiveAsync(fieldId, rule.Type, ct);
        if (hasActive)
        {
            ruleState.AlertActive = true;
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        // Cooldown também vale aqui (pra não spammar)
        var cooldown = TimeSpan.FromMinutes(rule.CooldownMinutes.GetValueOrDefault(0));
        var canFire =
            ruleState.LastTriggeredAtUtc is null ||
            cooldown == TimeSpan.Zero ||
            (measuredAtUtc - ruleState.LastTriggeredAtUtc.Value) >= cooldown;

        if (!canFire)
        {
            return;
        }

        var alert = new Alert
        {
            FieldId = fieldId,
            Type = rule.Type,
            Severity = rule.Severity,
            Message = RenderMessage(rule, measuredAtUtc, sum),
            TriggeredAtUtc = measuredAtUtc
        };

        await _alerts.AddAsync(alert, ct);

        ruleState.AlertActive = true;
        ruleState.LastTriggeredAtUtc = measuredAtUtc;
        ruleState.UpdatedAtUtc = measuredAtUtc;
    }

    /// <summary>
    /// Regra com duas métricas ao mesmo tempo (por duração).
    /// Ex: "umidade >= 70 E temperatura entre 20..32 por X minutos"
    /// 
    /// Aqui a gente só calcula a condição dupla e delega a lógica do tempo
    /// para o ApplyThresholdDurationAsync.
    /// </summary>
    private async Task ApplyDualMetricDurationAsync(AlertRule rule, RuleState ruleState, Guid fieldId, SensorReadingReceivedEvent evt, DateTime measuredAtUtc, CancellationToken ct)
    {
        if (rule.DurationMinutes is null || rule.DurationMinutes <= 0)
        {
            return;
        }

        if (rule.SecondaryMetric is null)
        {
            return;
        }

        var v1 = GetMetricValue(rule.Metric, evt);
        var cond1 = Compare(v1, rule.Operator, rule.ThresholdValue);

        var v2 = GetMetricValue(rule.SecondaryMetric.Value, evt);
        var min2 = rule.SecondaryMinValue ?? decimal.MinValue;
        var max2 = rule.SecondaryMaxValue ?? decimal.MaxValue;

        var cond2 = v2 >= min2 && v2 <= max2;

        var isInCondition = cond1 && cond2;

        // Reusa o mesmo mecanismo de "ficar X minutos na condição"
        await ApplyThresholdDurationAsync(rule, ruleState, fieldId, isInCondition, measuredAtUtc, ct);
    }

    /// <summary>
    /// Cria ou reaproveita a "memória" da regra para esse talhão.
    /// Chave real: (FieldId + RuleKey).
    /// </summary>
    private static RuleState GetOrCreateRuleState(FieldState state, string ruleKey)
    {
        var rs = state.Rules.FirstOrDefault(x => x.RuleKey == ruleKey);
        if (rs is not null)
        {
            return rs;
        }

        rs = new RuleState
        {
            FieldId = state.FieldId,
            RuleKey = ruleKey,
            UpdatedAtUtc = DateTime.UtcNow
        };

        state.Rules.Add(rs);
        return rs;
    }

    private static decimal GetMetricValue(SensorMetric metric, SensorReadingReceivedEvent evt) =>
        metric switch
        {
            SensorMetric.SoilMoisturePercent => evt.SoilMoisturePercent,
            SensorMetric.TemperatureC => evt.TemperatureC,
            SensorMetric.RainMm => evt.RainMm,
            _ => 0m
        };

    private static bool Compare(decimal left, ComparisonOp op, decimal right) =>
        op switch
        {
            ComparisonOp.LessThan => left < right,
            ComparisonOp.LessOrEqual => left <= right,
            ComparisonOp.GreaterThan => left > right,
            ComparisonOp.GreaterOrEqual => left >= right,
            _ => false
        };

    /// <summary>
    /// Monta a mensagem do alerta substituindo placeholders:
    /// {threshold}, {minutes}, {value}, {measuredAt}.
    /// </summary>
    private static string RenderMessage(AlertRule rule, DateTime measuredAtUtc, decimal? metricValue = null)
    {
        return (rule.MessageTemplate ?? string.Empty)
            .Replace("{threshold}", rule.ThresholdValue.ToString("0.##"))
            .Replace("{minutes}", (rule.DurationMinutes ?? 0).ToString())
            .Replace("{value}", metricValue?.ToString("0.##") ?? "")
            .Replace("{measuredAt}", measuredAtUtc.ToString("O"));
    }

    private static DateTime EnsureUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
