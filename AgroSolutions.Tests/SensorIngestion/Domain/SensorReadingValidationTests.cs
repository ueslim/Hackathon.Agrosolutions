using FIAP.AgroSolutions.SensorIngestion.Domain.Validation;
using Xunit;

namespace AgroSolutions.Tests.SensorIngestion.Domain;

/// <summary>
/// Unit tests for SensorIngestion SensorReading validation rules.
/// Validates fundamental business rules: Humidity cannot be negative, rain cannot be negative.
/// </summary>
public class SensorReadingValidationTests
{
    [Fact]
    public void IsValidSoilMoisture_WithNegativeValue_ReturnsFalse()
    {
        // Arrange - Humidity cannot be negative
        const decimal negativeMoisture = -1m;

        // Act
        var result = SensorReadingValidation.IsValidSoilMoisture(negativeMoisture);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(27.9)]
    [InlineData(100)]
    public void IsValidSoilMoisture_WithValidRange_ReturnsTrue(decimal moisture)
    {
        // Act
        var result = SensorReadingValidation.IsValidSoilMoisture(moisture);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Validate_WithNegativeSoilMoisture_ReturnsError()
    {
        // Arrange
        const decimal invalidMoisture = -5m;

        // Act
        var (isValid, error) = SensorReadingValidation.Validate(invalidMoisture, 36m, 0.1m);

        // Assert
        Assert.False(isValid);
        Assert.NotNull(error);
        Assert.Contains("negative", error);
    }

    [Fact]
    public void Validate_WithValidReading_ReturnsSuccess()
    {
        // Arrange - Example: "umidade 27.9%, temperatura 36°C, chuva 0.1mm"
        const decimal moisture = 27.9m;
        const decimal temp = 36m;
        const decimal rain = 0.1m;

        // Act
        var (isValid, error) = SensorReadingValidation.Validate(moisture, temp, rain);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }
}
