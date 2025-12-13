using InfraFlowSculptor.Api.Common.Mapping;
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
        services.AddOpenApiExtensions();
        services.AddValidation();
        return services;
    }
}