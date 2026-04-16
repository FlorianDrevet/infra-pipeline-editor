using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.DeleteUserAssignedIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UnlinkResourceFromIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UpdateUserAssignedIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Queries.GetUserAssignedIdentity;
using InfraFlowSculptor.Application.RoleAssignments.Queries.ListRoleAssignmentsByIdentity;
using InfraFlowSculptor.Contracts.UserAssignedIdentities.Requests;
using InfraFlowSculptor.Contracts.UserAssignedIdentities.Responses;
using InfraFlowSculptor.Contracts.RoleAssignments.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>
/// Minimal API endpoints for user-assigned managed identity CRUD operations.
/// </summary>
public static class UserAssignedIdentityController
{
    /// <summary>
    /// Maps the user-assigned identity endpoint group to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseUserAssignedIdentityController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/user-assigned-identity")
                .WithTags("User Assigned Identities");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetUserAssignedIdentityQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            identity =>
                            {
                                var response = mapper.Map<UserAssignedIdentityResponse>(identity);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetUserAssignedIdentity")
                .WithSummary("Get a User Assigned Identity")
                .WithDescription("Returns the full details of a single user-assigned managed identity resource.")
                .Produces<UserAssignedIdentityResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateUserAssignedIdentityRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateUserAssignedIdentityCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            identity =>
                            {
                                var response = mapper.Map<UserAssignedIdentityResponse>(identity);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetUserAssignedIdentity",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateUserAssignedIdentity")
                .WithSummary("Create a User Assigned Identity")
                .WithDescription("Creates a new user-assigned managed identity inside the specified Resource Group. Requires Owner or Contributor access.")
                .Produces<UserAssignedIdentityResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateUserAssignedIdentityRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateUserAssignedIdentityCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            identity =>
                            {
                                var response = mapper.Map<UserAssignedIdentityResponse>(identity);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateUserAssignedIdentity")
                .WithSummary("Update a User Assigned Identity")
                .WithDescription("Replaces all mutable properties of an existing user-assigned managed identity. Requires Owner or Contributor access.")
                .Produces<UserAssignedIdentityResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteUserAssignedIdentityCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteUserAssignedIdentity")
                .WithSummary("Delete a User Assigned Identity")
                .WithDescription("Permanently deletes a user-assigned managed identity resource. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}/granted-role-assignments",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListRoleAssignmentsByIdentityQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            assignments =>
                            {
                                var response = assignments
                                    .Select(a => mapper.Map<IdentityRoleAssignmentResponse>(a))
                                    .ToList();
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("ListGrantedRoleAssignments")
                .WithSummary("List role assignments granted through this identity")
                .WithDescription("Returns all RBAC role assignments across all resources that use this User-Assigned Identity. Requires read access.")
                .Produces<List<IdentityRoleAssignmentResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/{id:guid}/unlink-resource",
                    async ([FromRoute] Guid id, [FromBody] UnlinkResourceFromIdentityRequest request, IMediator mediator) =>
                    {
                        var command = new UnlinkResourceFromIdentityCommand(
                            new AzureResourceId(id),
                            new AzureResourceId(request.SourceResourceId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("UnlinkResourceFromIdentity")
                .WithSummary("Unlink a resource from this identity")
                .WithDescription(
                    "Removes the association between a source resource and this User-Assigned Identity. " +
                    "Role assignments are preserved on the identity itself; only the consuming resource's link is removed. " +
                    "Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
