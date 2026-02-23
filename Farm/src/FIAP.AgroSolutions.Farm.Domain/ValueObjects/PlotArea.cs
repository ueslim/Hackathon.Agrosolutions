namespace FIAP.AgroSolutions.Farm.Domain.ValueObjects;

/// <summary>
/// Value object representing the area of an agricultural plot in hectares.
/// Fundamental business rule: Plot must have an area greater than zero.
/// </summary>
public readonly record struct PlotArea
{
    public decimal Hectares { get; }

    private PlotArea(decimal hectares) => Hectares = hectares;

    /// <summary>
    /// Creates a valid PlotArea from the given value in hectares.
    /// </summary>
    /// <param name="hectares">Area in hectares.</param>
    /// <returns>A valid PlotArea value object.</returns>
    /// <exception cref="ArgumentException">Thrown when area is zero or negative.</exception>
    public static PlotArea Create(decimal hectares)
    {
        if (hectares <= 0)
            throw new ArgumentException("Plot area must be greater than zero.", nameof(hectares));

        return new PlotArea(hectares);
    }

    /// <summary>
    /// Validates whether the given area is valid (greater than zero).
    /// </summary>
    public static bool IsValid(decimal hectares) => hectares > 0;

    public override string ToString() => FormattableString.Invariant($"{Hectares} ha");
}
