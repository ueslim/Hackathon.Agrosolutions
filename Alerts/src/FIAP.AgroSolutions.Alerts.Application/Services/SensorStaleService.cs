using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Domain.Entities;
using FIAP.AgroSolutions.Alerts.Domain.Enums;

namespace FIAP.AgroSolutions.Alerts.Application.Services;

/// <summary>
/// Serviço responsável por detectar sensores "parados".
/// 
/// Exemplo humano:
/// "Se um campo não envia nenhuma leitura há mais de X minutos,
/// então gera um alerta SensorStale."
/// 
/// Diferente dos outros alertas, esse NÃO depende de um evento novo.
/// Ele precisa rodar periodicamente (job / cron / background).
/// </summary>
public class SensorStaleService
{
    private readonly IAlertRepository _alerts;
    private readonly IAlertRuleRepository _rules;
    private readonly IFieldStateRepository _states;
    private readonly IUnitOfWork _uow;

    public SensorStaleService(
        IAlertRepository alerts,
        IAlertRuleRepository rules,
        IFieldStateRepository states,
        IUnitOfWork uow)
    {
        _alerts = alerts;
        _rules = rules;
        _states = states;
        _uow = uow;
    }

    /// <summary>
    /// Executa a verificação de sensores inativos.
    /// 
    /// Fluxo:
    /// 1) Busca a regra SensorStale
    /// 2) Descobre quais campos estão sem leitura há tempo demais
    /// 3) Evita duplicar alertas ou violar cooldown
    /// 4) Cria alerta SensorStale quando necessário
    /// </summary>
    public async Task RunAsync(CancellationToken ct)
    {
        // Busca a regra SensorStale configurada no banco
        var rules = await _rules.GetEnabledAsync(ct);
        var staleRule = rules.FirstOrDefault(r => r.Type == AlertType.SensorStale);

        // Se não existir regra ou estiver mal configurada, não faz nada
        if (staleRule is null || staleRule.DurationMinutes is null || staleRule.DurationMinutes <= 0)
        {
            return;
        }

        var threshold = TimeSpan.FromMinutes(staleRule.DurationMinutes.Value);
        var cooldown = TimeSpan.FromMinutes(staleRule.CooldownMinutes.GetValueOrDefault(60));

        var nowUtc = DateTime.UtcNow;
        var olderThanUtc = nowUtc - threshold;

        // Busca campos cuja última leitura é mais antiga que o limite
        var staleFields = await _states.GetStaleAsync(olderThanUtc, ct);

        foreach (var fs in staleFields)
        {
            // Evita criar outro alerta se já existe um ativo
            var active = await _alerts.GetActiveByFieldAsync(fs.FieldId, ct);
            if (active.Any(a => a.Type == AlertType.SensorStale))
            {
                continue;
            }

            // Aplica cooldown: evita disparar várias vezes seguidas
            var all = await _alerts.GetByFieldAsync(fs.FieldId, ct);
            var last = all
                .Where(a => a.Type == AlertType.SensorStale)
                .OrderByDescending(a => a.TriggeredAtUtc)
                .FirstOrDefault();

            if (last != null && (nowUtc - last.TriggeredAtUtc) < cooldown)
            {
                continue;
            }

            // Monta mensagem explicando quando foi a última leitura
            var msg = (staleRule.MessageTemplate ?? "")
                .Replace("{minutes}", staleRule.DurationMinutes.Value.ToString())
                .Replace("{measuredAt}", (fs.LastReadingAtUtc?.ToString("O") ?? "n/a"));

            await _alerts.AddAsync(new Alert
            {
                FieldId = fs.FieldId,
                Type = AlertType.SensorStale,
                Severity = staleRule.Severity,
                Message = msg,
                TriggeredAtUtc = nowUtc
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }
}
