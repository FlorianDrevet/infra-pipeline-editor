namespace InfraFlowSculptor.Mcp.Common;

/// <summary>
/// Configures the shared HTTP middleware pipeline used by the MCP host.
/// </summary>
public static class McpHttpPipelineExtensions
{
    /// <summary>
    /// Applies the HTTP middleware required to secure and throttle the MCP host.
    /// </summary>
    /// <param name="application">The MCP web application.</param>
    /// <returns>The same application instance for fluent chaining.</returns>
    public static WebApplication UseMcpHttpPipeline(this WebApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.UseMiddleware<SecurityHeadersMiddleware>();

        if (!application.Environment.IsDevelopment())
        {
            application.UseHsts();
        }

        application.UseRouting();
        application.UseAuthentication();
        application.UseRateLimiter();
        application.UseAuthorization();

        return application;
    }
}