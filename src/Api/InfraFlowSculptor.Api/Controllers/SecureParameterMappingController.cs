using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.SecureParameterMappings.Commands.SetSecureParameterMapping;
using InfraFlowSculptor.Application.SecureParameterMappings.Queries.GetSecureParameterMappings;
using InfraFlowSculptor.Contracts.SecureParameterMappings.Requests;
using InfraFlowSculptor.Contracts.SecureParameterMappings.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>API endpoints for managing secure parameter mappings on Azure resources.</summary>
public static class SecureParameterMappingController
{
    /// <summary>Registers the secure parameter mapping endpoints.</summary>
    public static IApplicationBuilder UseSecureParameterMappingController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/azure-resources/{resourceId:guid}/secure-parameter-mappings")
                .WithTags("SecureParameterMappings");

            group.MapGet("",
                    async ([FromRoute] Guid resourceId, IMediator mediator) =>
                    {
                        var query = new GetSecureParameterMappingsQuery(new AzureResourceId(resourceId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            mappings => Results.Ok(mappings.Select(m => new SecureParameterMappingResponse(
                                m.Id.ToString(),
                                m.SecureParameterName,
                                m.VariableGroupId?.ToString(),
                                m.VariableGroupName,
                                m.PipelineVariableName)).ToList()),
                            errors => errors.Result());
                    })
                .WithName("ListSecureParameterMappings")
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPut("",
                    async ([FromRoute] Guid resourceId,
                        [FromBody] SetSecureParameterMappingRequest request,
                        IMediator mediator) =>
                    {
                        var command = new SetSecureParameterMappingCommand(
                            new AzureResourceId(resourceId),
                            request.SecureParameterName,
                            request.VariableGroupId.HasValue
                                ? new ProjectPipelineVariableGroupId(request.VariableGroupId.Value)
                                : null,
                            request.PipelineVariableName);

                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.Ok(),
                            errors => errors.Result());
                    })
                .WithName("SetSecureParameterMapping")
                .ProducesProblem(StatusCodes.Status401Unauthorized);
        });
    }
}
