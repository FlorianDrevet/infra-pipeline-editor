using System.Text.Json;
using System.Text.Json.Serialization;

namespace InfraFlowSculptor.Mcp.Common;

/// <summary>
/// Shared JSON serialization defaults for the MCP layer.
/// </summary>
internal static class McpJsonDefaults
{
    /// <summary>
    /// Default <see cref="JsonSerializerOptions"/> for all MCP JSON responses.
    /// </summary>
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Serializes a standard MCP error response.
    /// </summary>
    /// <param name="code">Machine-readable error code.</param>
    /// <param name="message">Human-readable error message.</param>
    /// <returns>JSON string with <c>error</c> and <c>message</c> properties.</returns>
    internal static string Error(string code, string message) =>
        JsonSerializer.Serialize(new { error = code, message }, SerializerOptions);
}
