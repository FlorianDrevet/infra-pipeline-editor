using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands;
using MediatR;
using InfraFlowSculptor.Contracts.InfrastructureConfig;
using MapsterMapper;

namespace InfraFlowSculptor.Api.Controllers;

public static class InfrastructureConfigController
{
    public static IApplicationBuilder UseInfrastructureConfigController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var config = endpoints.MapGroup("/infra-config")
                .WithTags("Infrastructure Configuration")
                .WithOpenApi();
            
            config.MapPost("",
                    async (CreateInfrastructureConfigRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new CreateInfrastructureConfigCommand(request.Name);
                        var result = await mediator.Send(command);

                        return result.Match(
                            infraConfig =>
                            {
                                var response = mapper.Map<InfrastructureConfigResponse>(infraConfig);
                                return Results.Created($"/infra-config/{response.Id}", response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateInfrastructureConfig")
                .WithOpenApi();
        });
    }
}