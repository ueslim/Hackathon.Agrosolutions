using FIAP.AgroSolutions.Farm.Domain.Entities;
using Xunit;

namespace AgroSolutions.Tests.Farm.Domain;

/// <summary>
/// Unit tests for Field entity creation and basic properties.
/// </summary>
public class FieldEntityTests
{
    [Fact]
    public void Field_CanBeCreated_WithValidData()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act
        var field = new Field
        {
            Id = Guid.NewGuid(),
            FarmId = farmId,
            OwnerUserId = ownerId,
            Name = "Talhão Norte",
            Crop = "Soja",
            BoundaryDescription = "Coordinates XYZ",
            CreatedAtUtc = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, field.Id);
        Assert.Equal(farmId, field.FarmId);
        Assert.Equal(ownerId, field.OwnerUserId);
        Assert.Equal("Talhão Norte", field.Name);
        Assert.Equal("Soja", field.Crop);
        Assert.Equal("Coordinates XYZ", field.BoundaryDescription);
    }

    [Fact]
    public void Field_BoundaryDescription_CanBeNull()
    {
        // Act
        var field = new Field
        {
            Id = Guid.NewGuid(),
            FarmId = Guid.NewGuid(),
            OwnerUserId = Guid.NewGuid(),
            Name = "Talhão Sul",
            Crop = "Milho",
            BoundaryDescription = null
        };

        // Assert
        Assert.Null(field.BoundaryDescription);
    }
}
