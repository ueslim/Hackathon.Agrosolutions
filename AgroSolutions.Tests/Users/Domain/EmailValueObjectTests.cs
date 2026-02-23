using FIAP.AgroSolutions.Users.Domain.ValueObjects;
using Xunit;

namespace AgroSolutions.Tests.Users.Domain;

/// <summary>
/// Unit tests for Email value object validation.
/// Validates fundamental business rule: Email format must be valid.
/// </summary>
public class EmailValueObjectTests
{
    [Fact]
    public void Create_WithValidEmail_ReturnsEmailValueObject()
    {
        // Arrange
        const string validEmail = "user@example.com";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        Assert.Equal(validEmail, email.Value);
    }

    [Fact]
    public void Create_WithValidEmailWithSubdomain_ReturnsEmailValueObject()
    {
        // Arrange
        const string validEmail = "admin@mail.company.co.uk";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        Assert.Equal(validEmail, email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ThrowsArgumentException(string? invalidEmail)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Create(invalidEmail!));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("missing-at-sign.com")]
    [InlineData("@missinglocal.com")]
    [InlineData("missingdomain@")]
    [InlineData("spaces in@email.com")]
    [InlineData("double@@at.com")]
    public void Create_WithInvalidFormat_ThrowsArgumentException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Create(invalidEmail));
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("test.user@domain.org", true)]
    [InlineData("user+tag@example.com", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("invalid", false)]
    [InlineData("@nodomain.com", false)]
    public void IsValidFormat_ReturnsExpectedResult(string? email, bool expectedValid)
    {
        // Act
        var result = Email.IsValidFormat(email);

        // Assert
        Assert.Equal(expectedValid, result);
    }

    [Fact]
    public void Create_WithWhitespaceSurrounding_TrimsValue()
    {
        // Arrange
        const string emailWithSpaces = "  user@example.com  ";

        // Act
        var email = Email.Create(emailWithSpaces);

        // Assert
        Assert.Equal("user@example.com", email.Value);
    }

    [Fact]
    public void ToString_ReturnsEmailValue()
    {
        // Arrange
        const string validEmail = "user@example.com";
        var email = Email.Create(validEmail);

        // Act
        var result = email.ToString();

        // Assert
        Assert.Equal(validEmail, result);
    }
}
