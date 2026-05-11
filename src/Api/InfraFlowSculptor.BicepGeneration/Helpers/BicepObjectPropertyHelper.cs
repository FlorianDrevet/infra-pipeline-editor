using System.Reflection;
using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Helpers;

/// <summary>
/// Resolves serialized property names and values for typed Bicep parameter objects.
/// </summary>
internal static class BicepObjectPropertyHelper
{
    /// <summary>
    /// Enumerates the public instance properties of a typed object using its serialized property names.
    /// </summary>
    /// <param name="source">The object to inspect.</param>
    /// <returns>The serialized property names and their current values.</returns>
    internal static IEnumerable<(string PropertyName, object? Value)> EnumerateSerializedProperties(object source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetIndexParameters().Length == 0)
            .Select(property => (ResolveSerializedPropertyName(property), property.GetValue(source)));
    }

    /// <summary>
    /// Resolves the serialized name of a property, honoring <see cref="JsonPropertyNameAttribute"/> when present.
    /// </summary>
    /// <param name="property">The property to inspect.</param>
    /// <returns>The serialized property name.</returns>
    internal static string ResolveSerializedPropertyName(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        return property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
    }
}