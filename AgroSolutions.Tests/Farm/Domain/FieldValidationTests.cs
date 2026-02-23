using FIAP.AgroSolutions.Farm.Domain.Validation;
using Xunit;

namespace AgroSolutions.Tests.Farm.Domain;

/// <summary>
/// Unit tests for Field validation rules.
/// Validates fundamental business rules: Field name and crop cannot be empty.
/// </summary>
public class FieldValidationTests
{
    [Theory]
    [InlineData("Talhão Norte")]
    [InlineData("A")]
    [InlineData("  valid name  ")]
    public void IsValidName_WithValidName_ReturnsTrue(string name)
    {
        // Act
        var result = FieldValidation.IsValidName(name);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidName_WithInvalidName_ReturnsFalse(string? name)
    {
        // Act
        var result = FieldValidation.IsValidName(name);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("Soja")]
    [InlineData("Milho")]
    [InlineData("Trigo")]
    public void IsValidCrop_WithValidCrop_ReturnsTrue(string crop)
    {
        // Act
        var result = FieldValidation.IsValidCrop(crop);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidCrop_WithInvalidCrop_ReturnsFalse(string? crop)
    {
        // Act
        var result = FieldValidation.IsValidCrop(crop);

        // Assert
        Assert.False(result);
    }
}
