namespace InfraFlowSculptor.Mcp.Common;

/// <summary>
/// Configuration options for the MCP HTTP server endpoint.
/// Bound from the <c>Mcp</c> configuration section.
/// </summary>
public sealed class McpOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Mcp";

    /// <summary>The default listen URL used by the MCP host.</summary>
    public const string DefaultListenUrl = "http://127.0.0.1:5258";

    /// <summary>The default route path used by the MCP endpoint.</summary>
    public const string DefaultRoute = "/mcp";

    /// <summary>
    /// The URL Kestrel will listen on. Defaults to <c>http://127.0.0.1:5258</c>.
    /// Override via <c>Mcp:ListenUrl</c> in configuration or the <c>MCP__LISTENURL</c> environment variable.
    /// </summary>
    public string ListenUrl { get; set; } = DefaultListenUrl;

    /// <summary>
    /// The route path on which the MCP endpoint is mounted. Defaults to <c>/mcp</c>.
    /// Override via <c>Mcp:Route</c> in configuration.
    /// </summary>
    public string Route { get; set; } = DefaultRoute;
}
