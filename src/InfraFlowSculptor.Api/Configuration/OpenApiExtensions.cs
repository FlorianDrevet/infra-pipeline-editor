namespace InfraFlowSculptor.Api.Configuration;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiExtensions(this IServiceCollection services)
    {
        services.AddOpenApi("v1");

        return services;
    }
}
