using FIAP.AgroSolutions.SensorIngestion.Application.DTOs;

namespace FIAP.AgroSolutions.SensorIngestion.Application.Services;

/// <summary>
/// Simulador de sensores.
/// 
/// Responsabilidade:
/// - Gerar leituras falsas (fake) de sensores para um Field.
/// - Criar histórico no tempo (leituras no passado).
/// - Permitir testar alertas sem sensores reais.
///
/// Exemplo mental:
/// "Finja que um sensor mediu dados a cada 1 minuto,
/// durante 50 minutos, seguindo um cenário específico
/// (seca, calor, chuva forte, etc)."
/// </summary>
public class SensorSimulatorService
{
    private readonly ReadingService _readings;

    // Gerador de números aleatórios compartilhado (mais eficiente)
    private static readonly Random _rng = Random.Shared;

    public SensorSimulatorService(ReadingService readings)
    {
        _readings = readings;
    }

    /// <summary>
    /// Gera várias leituras em sequência (burst), simulando histórico.
    ///
    /// Parâmetros importantes:
    /// - fieldId: talhão onde o sensor está instalado
    /// - count: quantas leituras gerar
    /// - step: intervalo entre leituras (ex: 1 minuto)
    /// - scenario: tipo de cenário climático ("drought", "heat", etc)
    /// </summary>
    public async Task GenerateBurstAsync(Guid fieldId, int count, TimeSpan step, string scenario, CancellationToken ct)
    {
        // Normaliza o cenário para evitar erro por maiúscula/espaço
        scenario = (scenario ?? "normal").Trim().ToLowerInvariant();

        // 👉 Criamos leituras NO PASSADO
        // Ex: 50 leituras * 1 minuto = começamos 50 minutos atrás
        var start = DateTime.UtcNow - TimeSpan.FromTicks(step.Ticks * count);

        // Valores base usados como "ponto médio" para gerar variação
        decimal baseMoisture = scenario switch
        {
            "drought" => 28m,
            "waterlogging" => 85m,
            _ => 55m
        };

        decimal baseTemp = scenario switch
        {
            "heat" => 36m,
            "cold" => 4m,
            _ => 26m
        };

        for (int i = 0; i < count; i++)
        {
            // Cada leitura acontece um pouco depois da anterior
            var measuredAt = start + TimeSpan.FromTicks(step.Ticks * i);

            // Gera os valores conforme o cenário escolhido
            var (m, t, r) = scenario switch
            {
                // SECA:
                // - Umidade sempre baixa (< 30)
                // - Pouquíssima chuva
                "drought" => (
                    Clamp(baseMoisture + Noise(-2, 2), 5, 29.5m),
                    Clamp(baseTemp + Noise(-1, 1), 20, 40),
                    Clamp(Noise(0m, 0.2m), 0m, 0.2m)
                ),

                // ENCHARCAMENTO:
                // - Umidade sempre alta (> 80)
                "waterlogging" => (
                    Clamp(baseMoisture + Noise(-2, 2), 75, 95),
                    Clamp(baseTemp + Noise(-1, 1), 20, 35),
                    Clamp(Noise(0, 3), 0, 10)
                ),

                // CALOR CONTÍNUO:
                // - Temperatura sempre >= 35
                "heat" => (
                    Clamp(baseMoisture + Noise(-2, 2), 40, 70),
                    Clamp(baseTemp + Noise(-0.5m, 1.5m), 34, 40),
                    Clamp(Noise(0, 2), 0, 5)
                ),

                // CHUVA FORTE:
                // - A cada 10 leituras, uma chuva grande (>= 25mm)
                "heavyrain" => (
                    Clamp(baseMoisture + Noise(-2, 2), 50, 90),
                    Clamp(baseTemp + Noise(-1, 1), 20, 35),
                    i % 10 == 0
                        ? 25m + Noise(0, 20)
                        : Clamp(Noise(0, 2), 0, 5)
                ),

                // RISCO DE DOENÇA:
                // - Umidade sempre alta
                // - Temperatura sempre entre 20 e 32
                "disease" => (
                   Clamp(80m + Noise(-3m, 3m), 70.1m, 95m),
                   Clamp(26m + Noise(-2m, 2m), 20m, 32m),
                   Clamp(Noise(0m, 2m), 0m, 5m)
                ),

                // SEM CHUVA:
                // - Quase nenhuma chuva ao longo do tempo
                "norain" => (
                   Clamp(55m + Noise(-3m, 3m), 40m, 70m),
                   Clamp(26m + Noise(-2m, 2m), 20m, 35m),
                   Clamp(Noise(0m, 0.2m), 0m, 0.2m)
                ),

                // 🌱 NORMAL:
                // - Valores variados, sem forçar alertas
                _ => (
                    Clamp(baseMoisture + Noise(-5, 5), 20, 90),
                    Clamp(baseTemp + Noise(-3, 3), 5, 40),
                    Clamp(Noise(0m, 0.5m), 0m, 0.5m)
                )
            };

            // Envia a leitura como se fosse um sensor real
            var req = new CreateReadingRequest(FieldId: fieldId, SoilMoisturePercent: m, TemperatureC: t, RainMm: r, MeasuredAtUtc: measuredAt);

            await _readings.CreateAsync(req, ct);
        }
    }

    /// <summary>
    /// Gera um valor aleatório entre min e max.
    ///
    /// Exemplo:
    /// Noise(-2, 2) => algo como -1.3, 0.8, 1.9
    /// Usado para dar "imperfeição" de sensor real.
    /// </summary>
    private static decimal Noise(decimal min, decimal max)
    {
        var v = (decimal)_rng.NextDouble(); // 0..1
        return min + (max - min) * v;
    }

    /// <summary>
    /// Garante que o valor fique dentro de um intervalo seguro.
    ///
    /// Exemplo:
    /// Clamp(120, 0, 100) => 100
    /// Clamp(-5, 0, 100)  => 0
    ///
    /// Evita gerar dados impossíveis.
    /// </summary>
    private static decimal Clamp(decimal v, decimal min, decimal max)
        => v < min ? min : (v > max ? max : v);
}
