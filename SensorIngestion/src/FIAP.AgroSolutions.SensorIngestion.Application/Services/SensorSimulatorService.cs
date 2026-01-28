using FIAP.AgroSolutions.SensorIngestion.Application.DTOs;

namespace FIAP.AgroSolutions.SensorIngestion.Application.Services;

public class SensorSimulatorService
{
    private readonly ReadingService _readings;
    private static readonly Random _rng = Random.Shared;

    public SensorSimulatorService(ReadingService readings)
    {
        _readings = readings;
    }

    public async Task GenerateBurstAsync(Guid fieldId, int count, TimeSpan step, string scenario, CancellationToken ct)
    {
        scenario = (scenario ?? "normal").Trim().ToLowerInvariant();

        var start = DateTime.UtcNow - TimeSpan.FromTicks(step.Ticks * count);

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
            var measuredAt = start + TimeSpan.FromTicks(step.Ticks * i);

            var (m, t, r) = scenario switch
            {
                "drought" => (
                    Clamp(baseMoisture + Noise(-2, 2), 5, 29.5m),
                    Clamp(baseTemp + Noise(-1, 1), 20, 40),
                    Clamp(Noise(0m, 0.2m), 0m, 0.2m)
                ),

                "waterlogging" => (
                    Clamp(baseMoisture + Noise(-2, 2), 75, 95),
                    Clamp(baseTemp + Noise(-1, 1), 20, 35),
                    Clamp(Noise(0, 3), 0, 10)
                ),

                "heat" => (
                    Clamp(baseMoisture + Noise(-2, 2), 40, 70),
                    Clamp(baseTemp + Noise(-0.5m, 1.5m), 34, 40),
                    Clamp(Noise(0, 2), 0, 5)
                ),

                "heavyrain" => (
                    Clamp(baseMoisture + Noise(-2, 2), 50, 90),
                    Clamp(baseTemp + Noise(-1, 1), 20, 35),
                    i % 10 == 0 ? 25m + Noise(0, 20) : Clamp(Noise(0, 2), 0, 5)
                ),

                "disease" => (
                   Clamp(80m + Noise(-3m, 3m), 70.1m, 95m),
                   Clamp(26m + Noise(-2m, 2m), 20m, 32m),
                   Clamp(Noise(0m, 2m), 0m, 5m)
                ),
                "norain" => (
                   Clamp(55m + Noise(-3m, 3m), 40m, 70m),
                   Clamp(26m + Noise(-2m, 2m), 20m, 35m),
                   Clamp(Noise(0m, 0.2m), 0m, 0.2m)
                ),

                _ => (
                    Clamp(baseMoisture + Noise(-5, 5), 20, 90),
                    Clamp(baseTemp + Noise(-3, 3), 5, 40),
                    Clamp(Noise(0m, 0.5m), 0m, 0.5m)
                )
            };

            var req = new CreateReadingRequest(
                FieldId: fieldId,
                SoilMoisturePercent: m,
                TemperatureC: t,
                RainMm: r,
                MeasuredAtUtc: measuredAt
            );

            await _readings.CreateAsync(req, ct);
        }
    }

    private static decimal Noise(decimal min, decimal max)
    {
        var v = (decimal)_rng.NextDouble();
        return min + (max - min) * v;
    }

    private static decimal Clamp(decimal v, decimal min, decimal max)
        => v < min ? min : (v > max ? max : v);
}
