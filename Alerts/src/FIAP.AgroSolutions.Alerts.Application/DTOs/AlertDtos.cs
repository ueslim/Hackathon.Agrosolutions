namespace FIAP.AgroSolutions.Alerts.Application.DTOs;

public record AlertResponse(Guid Id, Guid FieldId, string Type, string Severity, string Message, DateTime TriggeredAtUtc, DateTime? ResolvedAtUtc);
public record FieldStatusResponse(Guid FieldId, string Status, decimal? LastSoilMoisturePercent, DateTime? LastReadingAtUtc, DateTime UpdatedAtUtc, List<ActiveAlertItem> ActiveAlerts);
public record ActiveAlertItem(Guid Id, string Type, string Severity, string Message, DateTime TriggeredAtUtc);
