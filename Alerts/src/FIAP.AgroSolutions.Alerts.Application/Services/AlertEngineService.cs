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
        var receivedAtUtc = EnsureUtc(evt.ReceivedAtUtc);

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

        var state = await _states.GetAsync(evt.FieldId, ct)
            ?? new FieldState { FieldId = evt.FieldId };

        state.Rules ??= new List<RuleState>();

        state.LastReadingAtUtc = measuredAtUtc;
        state.LastSoilMoisturePercent = evt.SoilMoisturePercent;
        state.LastTemperatureC = evt.TemperatureC;
        state.LastRainMm = evt.RainMm;
        state.UpdatedAtUtc = receivedAtUtc;

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
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        if (ruleState.WindowStartUtc is null)
        {
            ruleState.WindowStartUtc = measuredAtUtc;
            ruleState.AlertActive = false;
            ruleState.UpdatedAtUtc = measuredAtUtc;
            return;
        }

        var duration = measuredAtUtc - ruleState.WindowStartUtc.Value;

        if (duration < window)
        {
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

        var alert = new Alert
        {
            FieldId = fieldId,
            Type = rule.Type,
            Severity = rule.Severity,
            Message = RenderMessage(rule, measuredAtUtc),
            TriggeredAtUtc = measuredAtUtc
        };

        await _alerts.AddAsync(alert, ct);

        ruleState.AlertActive = true;
        ruleState.LastTriggeredAtUtc = measuredAtUtc;

        ruleState.WindowStartUtc = measuredAtUtc;

        ruleState.UpdatedAtUtc = measuredAtUtc;
    }

    private async Task ApplyInstantCooldownAsync(AlertRule rule, RuleState ruleState, Guid fieldId, bool isInCondition, DateTime measuredAtUtc, decimal metricValue, CancellationToken ct)
    {
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

        var cooldown = TimeSpan.FromMinutes(rule.CooldownMinutes.GetValueOrDefault(0));
        var canFire =
            ruleState.LastTriggeredAtUtc is null ||
            cooldown == TimeSpan.Zero ||
            (measuredAtUtc - ruleState.LastTriggeredAtUtc.Value) >= cooldown;

        if (!canFire)
        {
            return;
        }

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

    private async Task ApplyWindowSumThresholdAsync(AlertRule rule, RuleState ruleState, Guid fieldId, DateTime measuredAtUtc, CancellationToken ct)
    {
        if (rule.DurationMinutes is null || rule.DurationMinutes <= 0)
        {
            return;
        }

        var window = TimeSpan.FromMinutes(rule.DurationMinutes.Value);
        var fromUtc = measuredAtUtc - window;

        var count = await _readings.CountReadingsAsync(fieldId, fromUtc, measuredAtUtc, ct);
        if (count < 10)
        {
            return;
        }

        var sum = await _readings.SumRainAsync(fieldId, fromUtc, measuredAtUtc, ct);
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
