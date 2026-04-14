using System.Reflection;
using Mapster;
using MapsterMapper;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Registers Mapster mapping configuration and mapper services.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddMapping(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        config.Compile();

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}