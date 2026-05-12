using InfraFlowSculptor.Api.Options;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace InfraFlowSculptor.Api.Configuration;

/// <summary>
/// Registers the API CORS policy from typed configuration options.
/// </summary>
public static class CorsServiceCollectionExtensions
{
    private const string RequestedWithHeaderName = "X-Requested-With";

    private static readonly string[] DefaultAllowedOrigins =
    [
        "http://localhost:4200",
    ];

    private static readonly string[] AllowedHeaders =
    [
        HeaderNames.ContentType,
        HeaderNames.Authorization,
        HeaderNames.Accept,
        RequestedWithHeaderName,
    ];

    private static readonly string[] AllowedMethods =
    [
        HttpMethods.Get,
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Delete,
        HttpMethods.Patch,
        HttpMethods.Options,
    ];

    /// <summary>
    /// Adds the configured CORS policy and binds its options from configuration.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ApiCorsOptions>()
            .Bind(configuration.GetSection(ApiCorsOptions.SectionName));

        services.AddCors();
        services.AddOptions<CorsOptions>()
            .Configure<IOptions<ApiCorsOptions>>((corsOptions, apiCorsOptions) =>
                ConfigureCorsOptions(corsOptions, apiCorsOptions.Value));

        return services;
    }

    private static void ConfigureCorsOptions(CorsOptions corsOptions, ApiCorsOptions apiCorsOptions)
    {
        var origins = apiCorsOptions.AllowedOrigins is { Length: > 0 }
            ? apiCorsOptions.AllowedOrigins
            : DefaultAllowedOrigins;

        corsOptions.AddDefaultPolicy(policy =>
        {
            policy
                .WithOrigins(origins)
                .WithMethods(AllowedMethods)
                .WithHeaders(AllowedHeaders)
                .AllowCredentials();
        });
    }
}