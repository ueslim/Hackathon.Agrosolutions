using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Queries;

/// <summary>
/// Consulta/persistência do histórico de leituras do sensor (SensorReadings).
/// 
/// O AlertEngine usa isso principalmente para regras que dependem de janela de tempo
/// (ex.: "somar chuva nos últimos X minutos" para NoRain).
/// </summary>
public class ReadingsQuery : IReadingsQuery
{
    private readonly AlertsDbContext _db;

    public ReadingsQuery(AlertsDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Salva uma leitura recebida, de forma idempotente.
    /// 
    /// Importante: o consumer pode reprocessar mensagens (ou receber duplicadas),
    /// então garantimos que o mesmo ReadingId não seja salvo duas vezes.
    /// </summary>
    public async Task AddAsync(SensorReading reading, CancellationToken ct)
    {
        // Idempotência por ReadingId (evita duplicar histórico e distorcer somas/contagens)
        var exists = await _db.SensorReadings.AnyAsync(x => x.Id == reading.Id, ct);
        if (exists)
        {
            return;
        }

        _db.SensorReadings.Add(reading);
    }

    /// <summary>
    /// Soma a chuva (RainMm) dentro de uma janela [fromUtc..toUtc] para um FieldId.
    /// 
    /// Exemplo de uso: NoRain dispara quando "soma de chuva na janela" ficar abaixo do limiar.
    /// </summary>
    public async Task<decimal> SumRainAsync(Guid fieldId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        fromUtc = EnsureUtc(fromUtc);
        toUtc = EnsureUtc(toUtc);

        // Usamos MeasuredAtUtc (momento da medição), não ReceivedAtUtc,
        // porque a regra é sobre o "clima real" no período, não sobre atraso de entrega.
        var sum = await _db.SensorReadings
            .AsNoTracking()
            .Where(x => x.FieldId == fieldId &&
                        x.MeasuredAtUtc >= fromUtc &&
                        x.MeasuredAtUtc <= toUtc)
            .Select(x => (decimal?)x.RainMm)
            .SumAsync(ct);

        return sum ?? 0m;
    }

    /// <summary>
    /// Conta quantas leituras existem dentro de uma janela [fromUtc..toUtc] para um FieldId.
    /// 
    /// Usado para garantir histórico mínimo antes de avaliar agregações
    /// (ex.: evitar disparar NoRain com poucas leituras).
    /// </summary>
    public Task<int> CountReadingsAsync(Guid fieldId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        fromUtc = EnsureUtc(fromUtc);
        toUtc = EnsureUtc(toUtc);

        return _db.SensorReadings
            .AsNoTracking()
            .CountAsync(x => x.FieldId == fieldId &&
                             x.MeasuredAtUtc >= fromUtc &&
                             x.MeasuredAtUtc <= toUtc, ct);
    }

    private static DateTime EnsureUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
