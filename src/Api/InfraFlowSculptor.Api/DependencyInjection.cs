using InfraFlowSculptor.Api.Common.Mapping;
using Microsoft.AspNetCore.Authorization;
using Shared.Api.Configuration.OpenApiTransformers;

namespace InfraFlowSculptor.Api;

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