using FIAP.AgroSolutions.SensorIngestion.Application.Abstractions;
using FIAP.AgroSolutions.SensorIngestion.Application.DTOs;
using FIAP.AgroSolutions.SensorIngestion.Domain.Entities;
using System.Text.Json;

namespace FIAP.AgroSolutions.SensorIngestion.Application.Services;

public class ReadingService
{
    private readonly IReadingRepository _readings;
    private readonly IOutboxWriter _outbox;
    private readonly IUnitOfWork _uow;

    public ReadingService(IReadingRepository readings, IOutboxWriter outbox, IUnitOfWork uow)
    {
        _readings = readings;
        _outbox = outbox;
        _uow = uow;
    }

    public async Task<ReadingResponse> CreateAsync(CreateReadingRequest req, CancellationToken ct)
    {
        Validate(req);

        var reading = new SensorReading
        {
            FieldId = req.FieldId,
            SoilMoisturePercent = req.SoilMoisturePercent,
            TemperatureC = req.TemperatureC,
            RainMm = req.RainMm,
            MeasuredAtUtc = req.MeasuredAtUtc.Kind == DateTimeKind.Utc ? req.MeasuredAtUtc : DateTime.SpecifyKind(req.MeasuredAtUtc, DateTimeKind.Utc),
            ReceivedAtUtc = DateTime.UtcNow
        };

        await _readings.AddAsync(reading, ct);

        var evt = new
        {
            eventType = "SensorReadingReceived",
            readingId = reading.Id,
            fieldId = reading.FieldId,
            soilMoisturePercent = reading.SoilMoisturePercent,
            temperatureC = reading.TemperatureC,
            rainMm = reading.RainMm,
            measuredAtUtc = reading.MeasuredAtUtc,
            receivedAtUtc = reading.ReceivedAtUtc
        };

        await _outbox.EnqueueAsync(type: "SensorReadingReceived", payload: JsonSerializer.Serialize(evt), occurredAtUtc: reading.ReceivedAtUtc, ct: ct);

        await _uow.SaveChangesAsync(ct);

        return new ReadingResponse(
            reading.Id, reading.FieldId, reading.SoilMoisturePercent, reading.TemperatureC,
            reading.RainMm, reading.MeasuredAtUtc, reading.ReceivedAtUtc
        );
    }

    public async Task<List<ReadingResponse>> GetByFieldAsync(Guid fieldId, DateTime? fromUtc, DateTime? toUtc, int take, CancellationToken ct)
    {
        take = take <= 0 ? 200 : Math.Min(take, 2000);

        var list = await _readings.GetByFieldAsync(fieldId, fromUtc, toUtc, take, ct);

        return list
            .OrderByDescending(x => x.MeasuredAtUtc)
            .Select(x => new ReadingResponse(
                x.Id, x.FieldId, x.SoilMoisturePercent, x.TemperatureC, x.RainMm, x.MeasuredAtUtc, x.ReceivedAtUtc
            )).ToList();
    }

    private static void Validate(CreateReadingRequest req)
    {
        if (req.FieldId == Guid.Empty)
        {
            throw new ArgumentException("FieldId is required");
        }

        if (req.SoilMoisturePercent < 0 || req.SoilMoisturePercent > 100)
        {
            throw new ArgumentException("SoilMoisturePercent must be between 0 and 100");
        }

        if (req.RainMm < 0)
        {
            throw new ArgumentException("RainMm must be >= 0");
        }

        if (req.MeasuredAtUtc == default)
        {
            throw new ArgumentException("MeasuredAtUtc is required");
        }
    }
}
