using FIAP.AgroSolutions.Alerts.Domain.Entities;
using Xunit;

namespace AgroSolutions.Tests.Alerts.Domain;

/// <summary>
/// Unit tests for SensorReading entity creation.
/// </summary>
public class SensorReadingEntityTests
{
    [Fact]
    public void SensorReading_CanBeCreated_WithValidData()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var measuredAt = DateTime.UtcNow.AddMinutes(-5);
        var receivedAt = DateTime.UtcNow;

        // Act
        var reading = new SensorReading
        {
            Id = Guid.NewGuid(),
            FieldId = fieldId,
            SoilMoisturePercent = 45.5m,
            TemperatureC = 28.3m,
            RainMm = 0m,
            MeasuredAtUtc = measuredAt,
            ReceivedAtUtc = receivedAt
        };

        // Assert
        Assert.NotEqual(Guid.Empty, reading.Id);
        Assert.Equal(fieldId, reading.FieldId);
        Assert.Equal(45.5m, reading.SoilMoisturePercent);
        Assert.Equal(28.3m, reading.TemperatureC);
        Assert.Equal(0m, reading.RainMm);
        Assert.Equal(measuredAt, reading.MeasuredAtUtc);
        Assert.Equal(receivedAt, reading.ReceivedAtUtc);
    }
}
