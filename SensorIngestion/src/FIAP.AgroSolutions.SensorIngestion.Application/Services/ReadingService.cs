using FIAP.AgroSolutions.SensorIngestion.Application.Abstractions;
using FIAP.AgroSolutions.SensorIngestion.Application.DTOs;
using FIAP.AgroSolutions.SensorIngestion.Domain.Entities;
using System.Text.Json;

namespace FIAP.AgroSolutions.SensorIngestion.Application.Services;

/// <summary>
/// Serviço de aplicação responsável por:
/// 1) Validar e salvar uma leitura de sensor (SensorReading).
/// 2) Registrar um evento "SensorReadingReceived" na Outbox.
/// 
/// A Outbox garante que o evento só será publicado se a gravação no banco também der certo
/// (evita “salvou no banco mas não avisou o Alerts”, ou “avisou mas não salvou”).
/// </summary>
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

    /// <summary>
    /// Cria uma leitura (persistência) e também agenda o evento para o RabbitMQ via Outbox.
    ///
    /// Fluxo mental simples:
    /// "Chegou uma leitura -> salva -> escreve evento -> commit"
    /// </summary>
    public async Task<ReadingResponse> CreateAsync(CreateReadingRequest req, CancellationToken ct)
    {
        Validate(req);

        // MeasuredAtUtc = quando o sensor mediu.
        // ReceivedAtUtc = quando nosso sistema recebeu (agora).
        var reading = new SensorReading
        {
            FieldId = req.FieldId,
            SoilMoisturePercent = req.SoilMoisturePercent,
            TemperatureC = req.TemperatureC,
            RainMm = req.RainMm,
            MeasuredAtUtc = req.MeasuredAtUtc.Kind == DateTimeKind.Utc
                ? req.MeasuredAtUtc
                : DateTime.SpecifyKind(req.MeasuredAtUtc, DateTimeKind.Utc),
            ReceivedAtUtc = DateTime.UtcNow
        };

        // 1) Persistimos a leitura no banco do SensorIngestion
        await _readings.AddAsync(reading, ct);

        // 2) Criamos o "contrato" do evento que o Alerts consome.
        //    Atenção: nomes em camelCase porque é JSON e você já está padronizando assim.
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

        // 3) Outbox: grava o evento no banco (para um publisher publicar depois).
        await _outbox.EnqueueAsync(
            type: "SensorReadingReceived",
            payload: JsonSerializer.Serialize(evt),
            occurredAtUtc: reading.ReceivedAtUtc,
            ct: ct
        );

        // 4) Commit único: leitura + outbox no mesmo "SaveChanges"
        await _uow.SaveChangesAsync(ct);

        return new ReadingResponse(
            reading.Id,
            reading.FieldId,
            reading.SoilMoisturePercent,
            reading.TemperatureC,
            reading.RainMm,
            reading.MeasuredAtUtc,
            reading.ReceivedAtUtc
        );
    }

    /// <summary>
    /// Consulta leituras de um Field.
    /// Útil para debug e demo: confirmar que o simulador realmente gerou histórico.
    /// </summary>
    public async Task<List<ReadingResponse>> GetByFieldAsync(
        Guid fieldId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int take,
        CancellationToken ct)
    {
        // limite pra não matar a API por acidente (Postman/Front)
        take = take <= 0 ? 200 : Math.Min(take, 2000);

        var list = await _readings.GetByFieldAsync(fieldId, fromUtc, toUtc, take, ct);

        return list
            .OrderByDescending(x => x.MeasuredAtUtc)
            .Select(x => new ReadingResponse(
                x.Id,
                x.FieldId,
                x.SoilMoisturePercent,
                x.TemperatureC,
                x.RainMm,
                x.MeasuredAtUtc,
                x.ReceivedAtUtc
            ))
            .ToList();
    }

    // Regras mínimas para impedir dados "impossíveis" de entrar no sistema.
    // Importante para demo/hackathon: isso dá credibilidade e evita alertas sem sentido.
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
