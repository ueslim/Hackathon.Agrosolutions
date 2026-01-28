using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Application.DTOs;
using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Domain.Enums;

namespace FIAP.AgroSolutions.Alerts.Application.Services;

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

    public async Task ProcessAsync(SensorReadingReceivedEvent evt, CancellationToken ct)
    {
        var measuredAtUtc = EnsureUtc(evt.MeasuredAtUtc);

        await _readings.AddAsync(new SensorReading
        {
            Id = evt.ReadingId,
            FieldId = evt.FieldId,
            SoilMoisturePercent = evt.SoilMoisturePercent,
            TemperatureC = evt.TemperatureC,
            RainMm = evt.RainMm,
            MeasuredAtUtc = measuredAtUtc,
            ReceivedAtUtc = measuredAtUtc
        }, ct);

        var state = await _states.GetAsync(evt.FieldId, ct)
            ?? new FieldState { FieldId = evt.FieldId };

        state.Rules ??= new List<RuleState>();

        state.LastReadingAtUtc = measuredAtUtc;
        state.LastSoilMoisturePercent = evt.SoilMoisturePercent;
        state.LastTemperatureC = evt.TemperatureC;
        state.LastRainMm = evt.RainMm;
        state.UpdatedAtUtc = DateTime.UtcNow;

        var rules = await _rules.GetEnabledAsync(ct);

        foreach (var rule in rules)
        {
            await ApplyRuleAsync(rule, state, evt, measuredAtUtc, ct);
        }

        await _states.UpsertAsync(state, ct);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task ApplyRuleAsync(AlertRule rule, FieldState state, SensorReadingReceivedEvent evt, DateTime measuredAtUtc, CancellationToken ct)
    {
        var ruleState = GetOrCreateRuleState(state, rule.RuleKey);

        var metricValue = GetMetricValue(rule.Metric, evt);
        var isInCondition = Compare(metricValue, rule.Operator, rule.ThresholdValue);

        switch (rule.Kind)
        {
            case RuleKind.ThresholdDuration:
                await ApplyThresholdDurationAsync(rule, ruleState, state.FieldId, isInCondition, measuredAtUtc, ct);
                break;

            case RuleKind.ThresholdInstantCooldown:
                await ApplyInstantCooldownAsync(rule, ruleState, state.FieldId, isInCondition, measuredAtUtc, metricValue, ct);
                break;

            case RuleKind.WindowSumThreshold:
                await ApplyWindowSumThresholdAsync(rule, ruleState, state.FieldId, measuredAtUtc, ct);
                break;

            case RuleKind.DualMetricDuration:
                await ApplyDualMetricDurationAsync(rule, ruleState, state.FieldId, evt, measuredAtUtc, ct);
                break;
        }
    }

    private async Task ApplyThresholdDurationAsync(AlertRule rule, RuleState ruleState, Guid fieldId, bool isInCondition, DateTime measuredAtUtc, CancellationToken ct)
    {
        if (rule.DurationMinutes is null || rule.DurationMinutes <= 0)
        {
            return;
        }

        var window = TimeSpan.FromMinutes(rule.DurationMinutes.Value);

        if (!isInCondition)
        {
            ruleState.WindowStartUtc = null;
            ruleState.AlertActive = false;
            ruleState.UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        if (ruleState.WindowStartUtc is null)
        {
            ruleState.WindowStartUtc = measuredAtUtc;
            ruleState.AlertActive = false;
            ruleState.UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        var duration = measuredAtUtc - ruleState.WindowStartUtc.Value;

        if (!ruleState.AlertActive && duration >= window)
        {
            var alert = new Alert
            {
                FieldId = fieldId,
                Type = rule.Type,
                Severity = rule.Severity,
                Message = RenderMessage(rule, measuredAtUtc),
                TriggeredAtUtc = DateTime.UtcNow
            };

            await _alerts.AddAsync(alert, ct);

            ruleState.AlertActive = true;
            ruleState.LastTriggeredAtUtc = measuredAtUtc;
            ruleState.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    private async Task ApplyInstantCooldownAsync(AlertRule rule, RuleState ruleState, Guid fieldId, bool isInCondition, DateTime measuredAtUtc, decimal metricValue, CancellationToken ct)
    {
        if (!isInCondition)
        {
            ruleState.AlertActive = false;
            ruleState.UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        var cooldown = TimeSpan.FromMinutes(rule.CooldownMinutes.GetValueOrDefault(0));
        var canFire =
            ruleState.LastTriggeredAtUtc is null ||
            cooldown == TimeSpan.Zero ||
            (measuredAtUtc - ruleState.LastTriggeredAtUtc.Value) >= cooldown;

        if (!canFire)
        {
            return;
        }

        // severidade dinâmica para chuva muito alta (sem mudar regra no banco)
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
            TriggeredAtUtc = DateTime.UtcNow
        };

        await _alerts.AddAsync(alert, ct);

        ruleState.AlertActive = true;
        ruleState.LastTriggeredAtUtc = measuredAtUtc;
        ruleState.UpdatedAtUtc = DateTime.UtcNow;
    }



    private async Task ApplyWindowSumThresholdAsync(AlertRule rule, RuleState ruleState, Guid fieldId, DateTime measuredAtUtc, CancellationToken ct)
    {
        if (rule.DurationMinutes is null || rule.DurationMinutes <= 0)
        {
            return;
        }

        var cooldown = TimeSpan.FromMinutes(rule.CooldownMinutes.GetValueOrDefault(0));
        var canFire =
            ruleState.LastTriggeredAtUtc is null ||
            cooldown == TimeSpan.Zero ||
            (measuredAtUtc - ruleState.LastTriggeredAtUtc.Value) >= cooldown;

        if (!canFire)
        {
            return;
        }

        var window = TimeSpan.FromMinutes(rule.DurationMinutes.Value);
        var fromUtc = measuredAtUtc - window;

        var count = await _readings.CountReadingsAsync(fieldId, fromUtc, measuredAtUtc, ct);

        // não avalia agregação se não tiver histórico mínimo
        if (count < 10)
        {
            return;
        }


        // Ex: soma de chuva últimos 7 dias
        var sum = await _readings.SumRainAsync(fieldId, fromUtc, measuredAtUtc, ct);

        var isInCondition = Compare(sum, rule.Operator, rule.ThresholdValue);

        if (!isInCondition)
        {
            ruleState.AlertActive = false;
            ruleState.UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        var alert = new Alert
        {
            FieldId = fieldId,
            Type = rule.Type,
            Severity = rule.Severity,
            Message = RenderMessage(rule, measuredAtUtc, sum),
            TriggeredAtUtc = DateTime.UtcNow
        };

        await _alerts.AddAsync(alert, ct);

        ruleState.AlertActive = true;
        ruleState.LastTriggeredAtUtc = measuredAtUtc;
        ruleState.UpdatedAtUtc = DateTime.UtcNow;
    }

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

        // cond 1: métrica principal (ex: umidade >= 70)
        var v1 = GetMetricValue(rule.Metric, evt);
        var cond1 = Compare(v1, rule.Operator, rule.ThresholdValue);

        // cond 2: faixa na secondary (ex: temp 20..32)
        var v2 = GetMetricValue(rule.SecondaryMetric!.Value, evt);
        var cond2 =
            v2 >= rule.SecondaryMinValue.GetValueOrDefault(decimal.MinValue) &&
            v2 <= rule.SecondaryMaxValue.GetValueOrDefault(decimal.MaxValue);

        var isInCondition = cond1 && cond2;

        await ApplyThresholdDurationAsync(rule, ruleState, fieldId, isInCondition, measuredAtUtc, ct);
    }

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
