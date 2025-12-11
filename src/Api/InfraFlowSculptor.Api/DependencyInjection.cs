using InfraFlowSculptor.Api.Common.Mapping;

namespace InfraFlowSculptor.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddMapping();
        services.AddAuthorization();
        services.AddOpenApiExtensions();
        services.AddValidation();
        return services;
    }
}