namespace InfraFlowSculptor.Mcp.Common;

/// <summary>
/// Configures the shared HTTP middleware pipeline used by the MCP host.
/// </summary>
public static class McpHttpPipelineExtensions
{
    private static readonly string PlainHttpSchemePrefix = Uri.UriSchemeHttp + Uri.SchemeDelimiter;
    private const string PlainHttpOutsideDevelopmentWarningMessage = "MCP ListenUrl '{ListenUrl}' uses plain HTTP outside Development. Protect the endpoint with HTTPS or a trusted reverse proxy.";

    /// <summary>
    /// Applies the HTTP middleware required to secure and throttle the MCP host.
    /// </summary>
    /// <param name="application">The MCP web application.</param>
    /// <returns>The same application instance for fluent chaining.</returns>
    public static WebApplication UseMcpHttpPipeline(this WebApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        LogPlainHttpOutsideDevelopmentWarning(application);

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

    private static void LogPlainHttpOutsideDevelopmentWarning(WebApplication application)
    {
        if (application.Environment.IsDevelopment())
        {
            return;
        }

        var mcpOptions = application.Services.GetService<Microsoft.Extensions.Options.IOptions<McpOptions>>()?.Value ?? new McpOptions();

        if (mcpOptions.ListenUrl.StartsWith(PlainHttpSchemePrefix, StringComparison.OrdinalIgnoreCase))
        {
            application.Logger.LogWarning(PlainHttpOutsideDevelopmentWarningMessage, mcpOptions.ListenUrl);
        }
    }
}