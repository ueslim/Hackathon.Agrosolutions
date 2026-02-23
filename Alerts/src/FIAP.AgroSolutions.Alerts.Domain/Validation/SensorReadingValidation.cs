namespace FIAP.AgroSolutions.Alerts.Domain.Validation;

/// <summary>
/// Validation rules for sensor readings.
/// Fundamental business rules: humidity in valid range, rain non-negative, etc.
/// </summary>
public static class SensorReadingValidation
{
    /// <summary>
    /// Soil moisture must be between 0 and 100 percent (inclusive).
    /// Humidity cannot be negative.
    /// </summary>
    public const decimal MinSoilMoisturePercent = 0;
    public const decimal MaxSoilMoisturePercent = 100;

    /// <summary>
    /// Rain measurement cannot be negative (mm).
    /// </summary>
    public const decimal MinRainMm = 0;

    /// <summary>
    /// Validates soil moisture percentage. Must be between 0 and 100.
    /// </summary>
    public static bool IsValidSoilMoisture(decimal soilMoisturePercent) =>
        soilMoisturePercent >= MinSoilMoisturePercent && soilMoisturePercent <= MaxSoilMoisturePercent;

    /// <summary>
    /// Validates rain measurement. Must be non-negative.
    /// </summary>
    public static bool IsValidRainMm(decimal rainMm) => rainMm >= MinRainMm;

    /// <summary>
    /// Validates a complete sensor reading.
    /// </summary>
    public static (bool IsValid, string? Error) Validate(decimal soilMoisturePercent, decimal temperatureC, decimal rainMm)
    {
        if (soilMoisturePercent < MinSoilMoisturePercent)
            return (false, "Soil moisture cannot be negative.");
        if (soilMoisturePercent > MaxSoilMoisturePercent)
            return (false, "Soil moisture cannot exceed 100 percent.");
        if (rainMm < MinRainMm)
            return (false, "Rain measurement cannot be negative.");
        return (true, null);
    }
}
