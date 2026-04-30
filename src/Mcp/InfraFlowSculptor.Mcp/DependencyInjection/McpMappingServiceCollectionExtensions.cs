using InfraFlowSculptor.Api.Common.Mapping;

namespace InfraFlowSculptor.Mcp.DependencyInjection;

/// <summary>
/// Registers mapping services required by application handlers invoked from the MCP host.
/// </summary>
public static class McpMappingServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Mapster mapping services used by MCP tools that dispatch application commands and queries.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMcpMappings(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMapping();
        return services;
    }
}