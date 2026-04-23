using System.Text;
using System.Text.RegularExpressions;

namespace InfraFlowSculptor.BicepGeneration.Helpers;

/// <summary>
/// Pure formatting and serialization utilities for Bicep string literals,
/// object keys, arrays, and type inference.
/// </summary>
internal static class BicepFormattingHelper
{
    /// <summary>
    /// Sanitizes a string for use as a Bicep object key, following the same camelCase convention
    /// as <see cref="BicepIdentifierHelper.ToBicepIdentifier"/>. Hyphens, underscores, and
    /// spaces are treated as word separators. Returns <c>"unknown"</c> when the input is empty
    /// or produces no parts.
    /// Example: <c>"my-env"</c> → <c>"myEnv"</c>, <c>"dev"</c> → <c>"dev"</c>.
    /// </summary>
    internal static string SanitizeBicepKey(string name)
    {
        if (string.IsNullOrEmpty(name)) return "unknown";
        var parts = name.Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "unknown";
        var sb = new StringBuilder(parts[0].ToLowerInvariant());
        foreach (var part in parts.Skip(1))
        {
            if (part.Length > 0)
                sb.Append(char.ToUpperInvariant(part[0])).Append(part[1..].ToLowerInvariant());
        }
        return sb.ToString();
    }

    internal static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    /// <summary>
    /// Escapes single quotes in a string for use within a Bicep string literal.
    /// </summary>
    internal static string EscapeBicepString(string value) =>
        value.Replace("'", "\\'");

    /// <summary>
    /// Formats a Bicep object key: returns the key unquoted if it is a valid Bicep identifier,
    /// or wraps it in single quotes otherwise (e.g. when it contains hyphens or spaces).
    /// </summary>
    internal static string FormatBicepObjectKey(string key) =>
        Regex.IsMatch(key, @"^[a-zA-Z_][a-zA-Z0-9_]*$")
            ? key
            : $"'{EscapeBicepString(key)}'";

    internal static string InferBicepType(object value)
    {
        return value switch
        {
            string => "string",
            int or long or double => "int",
            bool => "bool",
            System.Collections.IList => "array",
            _ => "object"
        };
    }

    internal static string SerializeToBicep(object value)
    {
        return value switch
        {
            string s => $"'{s}'",
            bool b => b ? "true" : "false",
            int or long or double => value.ToString()!,
            IList<object> list => SerializeArray(list),
            IDictionary<string, object> dict => SerializeDictionary(dict),
            _ => SerializeObject(value)
        };
    }

    internal static string SerializeObject(object obj)
    {
        var props = obj.GetType().GetProperties();

        var sb = new StringBuilder();
        sb.AppendLine("{");

        foreach (var p in props)
        {
            var propValue = p.GetValue(obj);
            if (propValue is not null)
                sb.AppendLine($"  {p.Name}: {SerializeToBicep(propValue)}");
        }

        sb.Append('}');
        return sb.ToString();
    }

    private static string SerializeDictionary(IDictionary<string, object> dict)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");

        foreach (var (key, value) in dict)
        {
            if (value is not null)
                sb.AppendLine($"  {key}: {SerializeToBicep(value)}");
        }

        sb.Append('}');
        return sb.ToString();
    }

    private static string SerializeArray(IList<object> items)
    {
        if (items.Count == 0)
            return "[]";

        var sb = new StringBuilder();
        sb.AppendLine("[");

        foreach (var item in items)
        {
            var serialized = SerializeToBicep(item);
            foreach (var line in serialized.Split('\n'))
            {
                sb.AppendLine($"  {line.TrimEnd()}");
            }
        }

        sb.Append(']');
        return sb.ToString();
    }

    internal static string RenderBicepStringArray(IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return "[]";
        }

        return $"[ {string.Join(", ", values.Select(v => $"'{EscapeBicepString(v)}'"))} ]";
    }
}
