using FIAP.AgroSolutions.Farm.Domain.ValueObjects;
using Xunit;

namespace AgroSolutions.Tests.Farm.Domain;

/// <summary>
/// Unit tests for PlotArea value object validation.
/// Validates fundamental business rule: Plot must have an area greater than zero.
/// </summary>
public class PlotAreaValueObjectTests
{
    [Fact]
    public void Create_WithPositiveArea_ReturnsPlotArea()
    {
        // Arrange
        const decimal hectares = 10.5m;

        // Act
        var plotArea = PlotArea.Create(hectares);

        // Assert
        Assert.Equal(hectares, plotArea.Hectares);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.001)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeArea_ThrowsArgumentException(decimal invalidArea)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => PlotArea.Create(invalidArea));
        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void Create_WithMinimumValidArea_Succeeds()
    {
        // Arrange - smallest positive value
        const decimal minArea = 0.0001m;

        // Act
        var plotArea = PlotArea.Create(minArea);

        // Assert
        Assert.Equal(minArea, plotArea.Hectares);
    }

    [Theory]
    [InlineData(0.1, true)]
    [InlineData(1, true)]
    [InlineData(100.5, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValid_ReturnsExpectedResult(decimal hectares, bool expectedValid)
    {
        // Act
        var result = PlotArea.IsValid(hectares);

        // Assert
        Assert.Equal(expectedValid, result);
    }

    [Fact]
    public void ToString_ReturnsFormattedArea()
    {
        // Arrange
        var plotArea = PlotArea.Create(25.5m);

        // Act
        var result = plotArea.ToString();

        // Assert
        // Use InvariantCulture formatting for consistent test across locales
        Assert.Contains("25.5", result, StringComparison.Ordinal);
        Assert.Contains("ha", result, StringComparison.Ordinal);
    }

    [Fact]
    public void ValueObject_Equality_WorksCorrectly()
    {
        // Arrange
        var area1 = PlotArea.Create(10m);
        var area2 = PlotArea.Create(10m);

        // Assert
        Assert.Equal(area1, area2);
        Assert.True(area1 == area2);
    }
}
