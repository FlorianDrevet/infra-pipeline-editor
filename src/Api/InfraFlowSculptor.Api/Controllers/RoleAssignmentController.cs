using InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Queries.ListRoleAssignments;
using InfraFlowSculptor.Contracts.RoleAssignments.Requests;
using InfraFlowSculptor.Contracts.RoleAssignments.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class RoleAssignmentController
{
    public static IApplicationBuilder UseRoleAssignmentController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/azure-resources/{resourceId:guid}/role-assignments")
                .WithTags("RoleAssignments");

            group.MapGet("",
                    async ([FromRoute] Guid resourceId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListRoleAssignmentsQuery(new AzureResourceId(resourceId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            assignments =>
                            {
                                var response = assignments
                                    .Select(a => mapper.Map<RoleAssignmentResponse>(a))
                                    .ToList();
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("ListRoleAssignments");

            group.MapPost("",
                    async ([FromRoute] Guid resourceId, [FromBody] AddRoleAssignmentRequest request,
                        IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<AddRoleAssignmentCommand>((resourceId, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            assignment =>
                            {
                                var response = mapper.Map<RoleAssignmentResponse>(assignment);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "ListRoleAssignments",
                                    routeValues: new { resourceId = resourceId },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("AddRoleAssignment");

            group.MapDelete("/{roleAssignmentId:guid}",
                    async ([FromRoute] Guid resourceId, [FromRoute] Guid roleAssignmentId, IMediator mediator) =>
                    {
                        var command = new RemoveRoleAssignmentCommand(
                            new AzureResourceId(resourceId),
                            new RoleAssignmentId(roleAssignmentId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveRoleAssignment");
        });
    }
}
