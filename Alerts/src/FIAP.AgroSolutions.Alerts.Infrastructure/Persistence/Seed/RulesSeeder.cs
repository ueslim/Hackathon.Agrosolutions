using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Persistence.Seed;

public static class RulesSeeder
{
    public static async Task SeedAsync(AlertsDbContext db, CancellationToken ct = default)
    {
        if (await db.AlertRules.AnyAsync(ct))
        {
            return;
        }

        var rules = new List<AlertRule>
        {
            // 1) Seca
            new AlertRule
            {
                RuleKey = "DroughtV1",
                Name = "Alerta de Seca (umidade < 30% por 24h)",
                IsEnabled = true,
                Type = AlertType.Drought,
                Severity = AlertSeverity.Warning,
                Kind = RuleKind.ThresholdDuration,
                Metric = SensorMetric.SoilMoisturePercent,
                Operator = ComparisonOp.LessThan,
                ThresholdValue = 30m,
                DurationMinutes = 24 * 60,
                MessageTemplate = "Umidade do solo abaixo de {threshold}% por mais de 24h."
            },

            // 2) Encharcamento / excesso de umidade
            new AlertRule
            {
                RuleKey = "WaterloggingV1",
                Name = "Encharcamento (umidade > 80% por 12h)",
                IsEnabled = true,
                Type = AlertType.Waterlogging,
                Severity = AlertSeverity.Warning,
                Kind = RuleKind.ThresholdDuration,
                Metric = SensorMetric.SoilMoisturePercent,
                Operator = ComparisonOp.GreaterThan,
                ThresholdValue = 80m,
                DurationMinutes = 12 * 60,
                MessageTemplate = "Umidade do solo acima de {threshold}% por 12h: risco de encharcamento/doenças."
            },

            // 3) Estresse térmico (calor)
            new AlertRule
            {
                RuleKey = "HeatStressV1",
                Name = "Estresse Térmico (temp >= 35°C por 2h)",
                IsEnabled = true,
                Type = AlertType.HeatStress,
                Severity = AlertSeverity.Warning,
                Kind = RuleKind.ThresholdDuration,
                Metric = SensorMetric.TemperatureC,
                Operator = ComparisonOp.GreaterOrEqual,
                ThresholdValue = 35m,
                DurationMinutes = 120,
                MessageTemplate = "Temperatura >= {threshold}°C por 2h: estresse térmico."
            },

            // 4) Estresse por frio
            new AlertRule
            {
                RuleKey = "ColdStressV1",
                Name = "Estresse por Frio (temp <= 5°C por 1h)",
                IsEnabled = true,
                Type = AlertType.ColdStress,
                Severity = AlertSeverity.Warning,
                Kind = RuleKind.ThresholdDuration,
                Metric = SensorMetric.TemperatureC,
                Operator = ComparisonOp.LessOrEqual,
                ThresholdValue = 5m,
                DurationMinutes = 60,
                MessageTemplate = "Temperatura <= {threshold}°C por 1h: estresse por frio."
            },

            // 5) Chuva forte
            new AlertRule
            {
                RuleKey = "HeavyRainV1",
                Name = "Chuva Forte (chuva >= 20mm) com cooldown",
                IsEnabled = true,
                Type = AlertType.HeavyRain,
                Severity = AlertSeverity.Warning,
                Kind = RuleKind.ThresholdInstantCooldown,
                Metric = SensorMetric.RainMm,
                Operator = ComparisonOp.GreaterOrEqual,
                ThresholdValue = 20m,
                CooldownMinutes = 360,
                MessageTemplate = "Chuva forte detectada: {value}mm (limiar {threshold}mm)."
            },

            // 6) Sem chuva (agregação em janela)
            new AlertRule
            {
                RuleKey = "NoRain7dV1",
                Name = "Sem chuva (soma 7 dias < 5mm)",
                IsEnabled = true,
                Type = AlertType.NoRain,
                Severity = AlertSeverity.Warning,
                Kind = RuleKind.WindowSumThreshold,
                Metric = SensorMetric.RainMm,
                Operator = ComparisonOp.LessThan,
                ThresholdValue = 5m,
                DurationMinutes = 7 * 24 * 60,
                CooldownMinutes = 24 * 60,
                MessageTemplate = "Pouca chuva: soma últimos 7 dias < {threshold}mm (soma={value}mm)."
            },

            // 7) DiseaseRisk proxy:
            new AlertRule
            {
                RuleKey = "DiseaseRiskV1",
                Name = "Risco de Doença/Praga (temp 20-32 e umidade >= 70 por 12h)",
                IsEnabled = true,
                Type = AlertType.DiseaseRisk,
                Severity = AlertSeverity.Warning,
                Kind = RuleKind.DualMetricDuration,
                Metric = SensorMetric.SoilMoisturePercent,
                Operator = ComparisonOp.GreaterOrEqual,
                ThresholdValue = 70m,
                DurationMinutes = 12 * 60,
                // parâmetros extras (você precisa ter estes campos no AlertRule)
                SecondaryMetric = SensorMetric.TemperatureC,
                SecondaryMinValue = 20m,
                SecondaryMaxValue = 32m,
                MessageTemplate = "Risco de doença: umidade >= {threshold}% e temperatura 20-32°C por 12h."
            },
        };

        db.AlertRules.AddRange(rules);
        await db.SaveChangesAsync(ct);
    }
}
