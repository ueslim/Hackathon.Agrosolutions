using FIAP.AgroSolutions.Users.Domain.Entities;
using Xunit;

namespace AgroSolutions.Tests.Users.Domain;

/// <summary>
/// Unit tests for User entity creation and basic properties.
/// </summary>
public class UserEntityTests
{
    [Fact]
    public void User_CanBeCreated_WithValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var user = new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            PasswordHash = "hashed_password_value",
            CreatedAtUtc = createdAt
        };

        // Assert
        Assert.Equal(userId, user.Id);
        Assert.Equal("John Doe", user.Name);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal("hashed_password_value", user.PasswordHash);
        Assert.Equal(createdAt, user.CreatedAtUtc);
        Assert.Null(user.UpdatedAtUtc);
    }

    [Fact]
    public void User_UpdatedAtUtc_CanBeSet()
    {
        // Arrange
        var updatedAt = DateTime.UtcNow;

        // Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Jane Doe",
            Email = "jane@example.com",
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedAtUtc = updatedAt
        };

        // Assert
        Assert.Equal(updatedAt, user.UpdatedAtUtc);
    }
}
