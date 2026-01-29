using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Persistence.Seed;

/// <summary>
/// Responsável por popular o banco com o conjunto inicial de regras de alerta.
/// 
/// Essas regras representam o conhecimento de domínio do sistema
/// (condições climáticas, hídricas e operacionais relevantes para o campo).
/// 
/// O seeding ocorre apenas se não existir nenhuma regra cadastrada.
/// </summary>
public static class RulesSeeder
{
    public static async Task SeedAsync(AlertsDbContext db, CancellationToken ct = default)
    {
        // Evita recriar regras se o banco já estiver populado
        if (await db.AlertRules.AnyAsync(ct))
        {
            return;
        }

        var rules = new List<AlertRule>
        {
            // --------------------------------------------------
            // 1) SECA
            // Regra clássica de estresse hídrico:
            // umidade do solo abaixo do limite por um período contínuo.
            // --------------------------------------------------
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
                DurationMinutes = 2, // reduzido para demo
                MessageTemplate = "Umidade do solo abaixo de {threshold}% por mais de 24h."
            },

            // --------------------------------------------------
            // 2) ENCHARCAMENTO
            // Excesso de umidade do solo por tempo prolongado,
            // aumentando risco de doenças e problemas radiculares.
            // --------------------------------------------------
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
                DurationMinutes = 2, // reduzido para demo
                MessageTemplate = "Umidade do solo acima de {threshold}% por 12h: risco de encharcamento/doenças."
            },

            // --------------------------------------------------
            // 3) ESTRESSE TÉRMICO (CALOR)
            // Temperaturas muito altas mantidas por um período contínuo.
            // --------------------------------------------------
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
                DurationMinutes = 2, // reduzido para demo
                MessageTemplate = "Temperatura >= {threshold}°C por 2h: estresse térmico."
            },

            // --------------------------------------------------
            // 4) ESTRESSE POR FRIO
            // Temperatura muito baixa mantida por tempo suficiente
            // para causar danos à cultura.
            // --------------------------------------------------
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

            // --------------------------------------------------
            // 5) CHUVA FORTE
            // Evento pontual: dispara imediatamente ao detectar
            // chuva intensa, respeitando cooldown.
            // --------------------------------------------------
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
                CooldownMinutes = 1,
                MessageTemplate = "Chuva forte detectada: {value}mm (limiar {threshold}mm)."
            },

            // --------------------------------------------------
            // 6) SEM CHUVA (NoRain)
            // Avalia a soma acumulada de chuva em uma janela de tempo.
            // Dispara quando o volume total é muito baixo.
            // --------------------------------------------------
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
                DurationMinutes = 10, // janela reduzida para demo
                CooldownMinutes = 1,
                MessageTemplate = "Pouca chuva: soma últimos 7 dias < {threshold}mm (soma={value}mm)."
            },

            // --------------------------------------------------
            // 7) RISCO DE DOENÇA / PRAGA
            // Regra combinada: alta umidade + temperatura favorável
            // mantidas por um período contínuo.
            // --------------------------------------------------
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
                DurationMinutes = 2, // reduzido para demo
                SecondaryMetric = SensorMetric.TemperatureC,
                SecondaryMinValue = 20m,
                SecondaryMaxValue = 32m,
                MessageTemplate = "Risco de doença: umidade >= {threshold}% e temperatura 20-32°C por 12h."
            },

            // --------------------------------------------------
            // 8) SENSOR PARADO
            // Alerta operacional que indica falha de comunicação
            // quando não há leitura por um longo período.
            // --------------------------------------------------
            new AlertRule
            {
                RuleKey = "SensorStaleV1",
                Name = "Sensor parado (sem leitura por 60min)",
                IsEnabled = true,
                Type = AlertType.SensorStale,
                Severity = AlertSeverity.Info,
                Kind = RuleKind.ThresholdDuration,
                Metric = SensorMetric.SoilMoisturePercent, // métrica irrelevante aqui
                Operator = ComparisonOp.GreaterOrEqual,
                ThresholdValue = 0m,
                DurationMinutes = 60,
                CooldownMinutes = 5,
                MessageTemplate = "Sensor sem enviar leituras há {minutes} minutos. Última leitura: {measuredAt}."
            },
        };

        db.AlertRules.AddRange(rules);
        await db.SaveChangesAsync(ct);
    }
}
