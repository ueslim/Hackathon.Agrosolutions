namespace FIAP.AgroSolutions.Farm.Domain.Validation;

/// <summary>
/// Validation rules for agricultural fields (plots).
/// Fundamental business rules: field name required, area validation.
/// </summary>
public static class FieldValidation
{
    /// <summary>
    /// Field name cannot be null or whitespace.
    /// </summary>
    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    /// <summary>
    /// Crop cannot be null or whitespace.
    /// </summary>
    public static bool IsValidCrop(string? crop) => !string.IsNullOrWhiteSpace(crop);
}
