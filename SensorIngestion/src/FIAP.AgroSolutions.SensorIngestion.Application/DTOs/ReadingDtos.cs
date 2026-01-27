namespace FIAP.AgroSolutions.SensorIngestion.Application.DTOs;

public record CreateReadingRequest(Guid FieldId, decimal SoilMoisturePercent, decimal TemperatureC, decimal RainMm, DateTime MeasuredAtUtc);

public record ReadingResponse(Guid Id, Guid FieldId, decimal SoilMoisturePercent, decimal TemperatureC, decimal RainMm, DateTime MeasuredAtUtc, DateTime ReceivedAtUtc);
