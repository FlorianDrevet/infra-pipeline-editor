using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.Common.Queries.DetectPipelineOptions;
using InfraFlowSculptor.Contracts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>API endpoint for auto-detecting pipeline options from a compute resource's repository.</summary>
public static class PipelineOptionDetectionController
{
    /// <summary>Registers the pipeline option detection endpoints.</summary>
    public static IApplicationBuilder UsePipelineOptionDetectionController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.AzureResourceDetectPipelineOptions)
                .WithTags("PipelineOptions");

            group.MapGet("",
                    async ([FromRoute] Guid resourceId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new DetectPipelineOptionsQuery(new AzureResourceId(resourceId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            detected => Results.Ok(mapper.Map<DetectedPipelineOptionsResponse>(detected)),
                            errors => errors.Result());
                    })
                .WithName(PipelineOptionDetectionRouteNames.DetectPipelineOptions)
                .ProducesProblem(StatusCodes.Status401Unauthorized);
        });
    }
}
