using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using InfraFlowSculptor.Application.Common.Behaviors;

namespace InfraFlowSculptor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // CQRS with MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly, Assembly.GetExecutingAssembly()));

        // Behaviors
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}