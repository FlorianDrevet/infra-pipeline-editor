using InfraFlowSculptor.Api.Common.Mapping;

namespace InfraFlowSculptor.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddMapping();
        services.AddAuthorization();
        return services;
    }
}