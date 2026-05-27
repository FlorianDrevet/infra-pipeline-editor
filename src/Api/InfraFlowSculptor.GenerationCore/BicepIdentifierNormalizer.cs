using System.Text;

namespace InfraFlowSculptor.GenerationCore;

/// <summary>
/// Provides the canonical camelCase normalization strategy used for generated Bicep identifiers and keys.
/// </summary>
public static class BicepIdentifierNormalizer
{
    /// <summary>
    /// Normalizes the provided value to camelCase by treating hyphens, underscores, and spaces as separators.
    /// Returns <paramref name="fallbackValue"/> when the input is empty or produces no identifier parts.
    /// </summary>
    /// <param name="value">The raw value to normalize.</param>
    /// <param name="fallbackValue">The fallback value to return when normalization cannot produce an identifier.</param>
    /// <returns>The normalized camelCase identifier.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fallbackValue"/> is null, empty, or whitespace.</exception>
    public static string NormalizeCamelCase(string? value, string fallbackValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fallbackValue);

        if (string.IsNullOrWhiteSpace(value))
            return fallbackValue;

        var parts = value.Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return fallbackValue;

        var stringBuilder = new StringBuilder(parts[0].ToLowerInvariant());
        foreach (var part in parts.Skip(1))
        {
            if (part.Length == 0)
                continue;

            stringBuilder.Append(char.ToUpperInvariant(part[0]));

            if (part.Length > 1)
                stringBuilder.Append(part[1..].ToLowerInvariant());
        }

        return stringBuilder.ToString();
    }
}
