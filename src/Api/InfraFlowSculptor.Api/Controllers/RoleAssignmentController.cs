using InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;
using InfraFlowSculptor.Application.RoleAssignments.Queries.ListAvailableRoleDefinitions;
using InfraFlowSculptor.Application.RoleAssignments.Queries.ListRoleAssignments;
using InfraFlowSculptor.Contracts.RoleAssignments.Requests;
using InfraFlowSculptor.Contracts.RoleAssignments.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

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
                .WithName("ListRoleAssignments")
                .AddOpenApiOperationTransformer((operation, context, ct) =>
                {
                    operation.Summary = "List role assignments for a resource";
                    operation.Description =
                        "Returns all RBAC role assignments where the specified Azure resource is the source (identity bearer). " +
                        "Each entry describes the target resource, the managed identity type (SystemAssigned or UserAssigned), " +
                        "and the Azure role definition ID granted.";
                    return Task.CompletedTask;
                });

            group.MapGet("/available-role-definitions",
                    async ([FromRoute] Guid resourceId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListAvailableRoleDefinitionsQuery(new AzureResourceId(resourceId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            roles =>
                            {
                                var response = roles
                                    .Select(r => mapper.Map<AzureRoleDefinitionResponse>(r))
                                    .ToList();
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("ListAvailableRoleDefinitions")
                .AddOpenApiOperationTransformer((operation, context, ct) =>
                {
                    operation.Summary = "List available role definitions for a resource type";
                    operation.Description =
                        "Returns the Azure built-in RBAC role definitions applicable to the type of the specified resource " +
                        "(e.g. Key Vault roles for a KeyVault resource, Redis Cache roles for a RedisCache resource). " +
                        "Each entry includes the role definition ID, display name, description, and a link to the Azure documentation. " +
                        "Use the role definition ID from this list when calling the POST endpoint to add a role assignment.";
                    return Task.CompletedTask;
                });

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
                .WithName("AddRoleAssignment")
                .AddOpenApiOperationTransformer((operation, context, ct) =>
                {
                    operation.Summary = "Add a role assignment to a resource";
                    operation.Description =
                        "Assigns an Azure RBAC role to a target resource using the managed identity of the source resource. " +
                        "Specify 'SystemAssigned' or 'UserAssigned' for the managed identity type, and provide a valid " +
                        "Azure role definition ID applicable to the source resource type. " +
                        "Use GET /available-role-definitions to retrieve the list of valid role definition IDs for the resource. " +
                        "Duplicate assignments (same target, role, and identity type) are silently ignored.";
                    return Task.CompletedTask;
                });

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
                .WithName("RemoveRoleAssignment")
                .AddOpenApiOperationTransformer((operation, context, ct) =>
                {
                    operation.Summary = "Remove a role assignment from a resource";
                    operation.Description =
                        "Deletes the specified role assignment from the source resource. " +
                        "The caller must have write access (Owner or Contributor) on the infrastructure configuration " +
                        "that owns the resource. Returns 204 No Content on success.";
                    return Task.CompletedTask;
                });

            group.MapPut("/{roleAssignmentId:guid}/identity",
                    async ([FromRoute] Guid resourceId, [FromRoute] Guid roleAssignmentId,
                        [FromBody] UpdateRoleAssignmentIdentityRequest request,
                        IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateRoleAssignmentIdentityCommand>(
                            (resourceId, roleAssignmentId, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            assignment =>
                            {
                                var response = mapper.Map<RoleAssignmentResponse>(assignment);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateRoleAssignmentIdentity")
                .AddOpenApiOperationTransformer((operation, context, ct) =>
                {
                    operation.Summary = "Update the managed identity on a role assignment";
                    operation.Description =
                        "Changes the managed identity type (SystemAssigned or UserAssigned) and optional User-Assigned Identity " +
                        "on an existing role assignment. The caller must have write access (Owner or Contributor) on the infrastructure configuration.";
                    return Task.CompletedTask;
                });
        });
    }
}

