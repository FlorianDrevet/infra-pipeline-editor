using InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Commands.AssignIdentityToResource;
using InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Commands.UnassignIdentityFromResource;
using InfraFlowSculptor.Application.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;
using InfraFlowSculptor.Application.RoleAssignments.Queries.AnalyzeRoleAssignmentImpact;
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
    public static IEndpointRouteBuilder MapRoleAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/azure-resources/{resourceId:guid}/role-assignments")
            .WithTags("RoleAssignments");

        group.MapGet("",
                async ([FromRoute] Guid resourceId, IMediator mediator, IMapper mapper) =>
                {
                    var query = new ListRoleAssignmentsQuery(new AzureResourceId(resourceId));
                    var result = await mediator.Send(query);

                    return result.Match(
                        data =>
                        {
                            var roleAssignments = data.RoleAssignments
                                .Select(a => mapper.Map<RoleAssignmentResponse>(a))
                                .ToList();
                            var response = new RoleAssignmentsWithIdentityResponse(
                                data.AssignedUserAssignedIdentityId,
                                data.AssignedUserAssignedIdentityName,
                                roleAssignments);
                            return TypedResults.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("ListRoleAssignments")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Remove a role assignment from a resource";
                operation.Description =
                    "Deletes the specified role assignment from the source resource. " +
                    "The caller must have write access (Owner or Contributor) on the infrastructure configuration " +
                    "that owns the resource. Returns 204 No Content on success.";
                return Task.CompletedTask;
            });

        group.MapGet("/{roleAssignmentId:guid}/impact-analysis",
                async ([FromRoute] Guid resourceId, [FromRoute] Guid roleAssignmentId,
                    IMediator mediator, IMapper mapper) =>
                {
                    var query = new AnalyzeRoleAssignmentImpactQuery(
                        new AzureResourceId(resourceId),
                        new RoleAssignmentId(roleAssignmentId));
                    var result = await mediator.Send(query);

                    return result.Match(
                        impact =>
                        {
                            var response = mapper.Map<RoleAssignmentImpactResponse>(impact);
                            return TypedResults.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("AnalyzeRoleAssignmentImpact")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Analyze the impact of removing a role assignment";
                operation.Description =
                    "Returns the potential impact of removing the specified role assignment. " +
                    "Checks for broken container image pulls (AcrPull), inaccessible Key Vault secrets, " +
                    "and whether this is the last role to the target resource. " +
                    "Call this endpoint before deleting a role assignment to warn the user about potential issues.";
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Update the managed identity on a role assignment";
                operation.Description =
                    "Changes the managed identity type (SystemAssigned or UserAssigned) and optional User-Assigned Identity " +
                    "on an existing role assignment. The caller must have write access (Owner or Contributor) on the infrastructure configuration.";
                return Task.CompletedTask;
            });

        // Assigned Identity endpoints
        var identityGroup = app.MapGroup("/azure-resources/{resourceId:guid}/assigned-identity")
            .WithTags("AssignedIdentity");

        identityGroup.MapPut("",
                async ([FromRoute] Guid resourceId, [FromBody] AssignIdentityToResourceRequest request,
                    IMediator mediator, IMapper mapper) =>
                {
                    var command = mapper.Map<AssignIdentityToResourceCommand>((resourceId, request));
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName("AssignIdentityToResource")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Assign a User-Assigned Identity to a resource";
                operation.Description =
                    "Attaches a User-Assigned Identity to the specified resource (ARM identity.userAssignedIdentities concept). " +
                    "Any UserAssigned role assignments that duplicate a SystemAssigned role assignment (same target + same role) " +
                    "are automatically removed. Returns 204 No Content on success.";
                return Task.CompletedTask;
            });

        identityGroup.MapDelete("",
                async ([FromRoute] Guid resourceId, IMediator mediator) =>
                {
                    var command = new UnassignIdentityFromResourceCommand(new AzureResourceId(resourceId));
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName("UnassignIdentityFromResource")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Remove the assigned User-Assigned Identity from a resource";
                operation.Description =
                    "Detaches the User-Assigned Identity from the specified resource. " +
                    "This does not remove any role assignments — it only clears the identity link. " +
                    "Returns 204 No Content on success.";
                return Task.CompletedTask;
            });
        return app;
    }
}

