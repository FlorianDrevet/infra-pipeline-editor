using System.Text.Json;
using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Helpers;

/// <summary>
/// Converts typed parameter models into the legacy dictionary/object graph expected by module assemblers.
/// </summary>
internal static class BicepParameterModelConverter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Converts a typed parameter model into a dictionary keyed by its serialized property names.
    /// </summary>
    /// <typeparam name="TModel">The parameter model type.</typeparam>
    /// <param name="model">The typed model to convert.</param>
    /// <returns>The converted dictionary.</returns>
    internal static IReadOnlyDictionary<string, object> ToDictionary<TModel>(TModel model)
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(model);

        var element = JsonSerializer.SerializeToElement(model, SerializerOptions);
        return ConvertObjectElement(element);
    }

    /// <summary>
    /// Converts a typed parameter model into a single legacy object value.
    /// </summary>
    /// <typeparam name="TModel">The parameter model type.</typeparam>
    /// <param name="model">The typed model to convert.</param>
    /// <returns>The converted legacy value.</returns>
    internal static object ToValue<TModel>(TModel model)
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(model);

        var element = JsonSerializer.SerializeToElement(model, SerializerOptions);
        return ConvertElement(element);
    }

    private static Dictionary<string, object> ConvertObjectElement(JsonElement element)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = ConvertElement(property.Value);
        }

        return result;
    }

    private static object ConvertElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertObjectElement(element),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertElement).ToList(),
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
            JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.Number when element.TryGetInt64(out var longValue) => longValue,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.Null => throw new InvalidOperationException("Null values must be omitted before conversion."),
            _ => throw new InvalidOperationException($"Unsupported JSON value kind '{element.ValueKind}'."),
        };
    }
}