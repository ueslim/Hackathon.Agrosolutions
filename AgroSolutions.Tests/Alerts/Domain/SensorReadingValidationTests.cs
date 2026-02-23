using FIAP.AgroSolutions.Alerts.Domain.Validation;
using Xunit;

namespace AgroSolutions.Tests.Alerts.Domain;

/// <summary>
/// Unit tests for SensorReading validation rules.
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
    [InlineData(50)]
    [InlineData(100)]
    public void IsValidSoilMoisture_WithValidRange_ReturnsTrue(decimal moisture)
    {
        // Act
        var result = SensorReadingValidation.IsValidSoilMoisture(moisture);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidSoilMoisture_WithValueAbove100_ReturnsFalse()
    {
        // Arrange
        const decimal excessMoisture = 100.1m;

        // Act
        var result = SensorReadingValidation.IsValidSoilMoisture(excessMoisture);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidRainMm_WithNegativeValue_ReturnsFalse()
    {
        // Arrange - Rain measurement cannot be negative
        const decimal negativeRain = -0.1m;

        // Act
        var result = SensorReadingValidation.IsValidRainMm(negativeRain);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5.5)]
    [InlineData(100)]
    public void IsValidRainMm_WithNonNegativeValue_ReturnsTrue(decimal rainMm)
    {
        // Act
        var result = SensorReadingValidation.IsValidRainMm(rainMm);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Validate_WithNegativeSoilMoisture_ReturnsError()
    {
        // Arrange
        const decimal invalidMoisture = -5m;

        // Act
        var (isValid, error) = SensorReadingValidation.Validate(invalidMoisture, 25m, 0m);

        // Assert
        Assert.False(isValid);
        Assert.NotNull(error);
        Assert.Contains("negative", error);
    }

    [Fact]
    public void Validate_WithNegativeRain_ReturnsError()
    {
        // Arrange
        const decimal invalidRain = -1m;

        // Act
        var (isValid, error) = SensorReadingValidation.Validate(50m, 25m, invalidRain);

        // Assert
        Assert.False(isValid);
        Assert.NotNull(error);
        Assert.Contains("negative", error);
    }

    [Fact]
    public void Validate_WithValidReading_ReturnsSuccess()
    {
        // Arrange
        const decimal moisture = 45m;
        const decimal temp = 28.5m;
        const decimal rain = 0m;

        // Act
        var (isValid, error) = SensorReadingValidation.Validate(moisture, temp, rain);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Constants_DefineValidRanges()
    {
        // Assert - Constants used by validation
        Assert.Equal(0, SensorReadingValidation.MinSoilMoisturePercent);
        Assert.Equal(100, SensorReadingValidation.MaxSoilMoisturePercent);
        Assert.Equal(0, SensorReadingValidation.MinRainMm);
    }
}
