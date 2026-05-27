using InfraFlowSculptor.Api.Options;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Api.Configuration;

/// <summary>
/// Registers the request body size limits used by the API.
/// </summary>
public static class RequestLimitsServiceCollectionExtensions
{
    /// <summary>
    /// Adds the configured API request body size limits and validates the bound configuration at startup.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApiRequestLimits(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ApiRequestLimitsOptions>()
            .Bind(configuration.GetSection(ApiRequestLimitsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<KestrelServerOptions>()
            .Configure<IOptions<ApiRequestLimitsOptions>>((kestrelOptions, apiRequestLimitsOptions) =>
                kestrelOptions.Limits.MaxRequestBodySize = apiRequestLimitsOptions.Value.MaxRequestBodySizeBytes);

        return services;
    }
}
