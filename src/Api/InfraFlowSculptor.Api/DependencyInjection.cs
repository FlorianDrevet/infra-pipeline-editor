using InfraFlowSculptor.Api.Common.Mapping;
using InfraFlowSculptor.Api.Configuration.OpenApiTransformers;
using Microsoft.AspNetCore.Authorization;

namespace InfraFlowSculptor.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddMapping();
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<SecuritySchemeTransformer>();
        });

        services.AddValidation();
        return services;
    }
}