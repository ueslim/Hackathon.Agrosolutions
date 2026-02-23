using System.Text.RegularExpressions;

namespace FIAP.AgroSolutions.Users.Domain.ValueObjects;

/// <summary>
/// Value object representing a valid email address.
/// Ensures email format validation according to fundamental business rules.
/// </summary>
public readonly record struct Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    /// <summary>
    /// Creates a valid Email from the given string.
    /// </summary>
    /// <param name="value">The email address string.</param>
    /// <returns>A valid Email value object.</returns>
    /// <exception cref="ArgumentException">Thrown when the email format is invalid.</exception>
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty.", nameof(value));

        if (!IsValidFormat(value))
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));

        return new Email(value.Trim());
    }

    /// <summary>
    /// Validates whether the given string has a valid email format.
    /// </summary>
    public static bool IsValidFormat(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email.Trim());
    }

    public override string ToString() => Value;
}
