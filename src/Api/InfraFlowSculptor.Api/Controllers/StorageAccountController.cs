using InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddQueue;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Commands.DeleteStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveBlobContainer;
using InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveQueue;
using InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveTable;
using InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateBlobContainerPublicAccess;
using InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Queries;
using InfraFlowSculptor.Contracts.StorageAccounts.Requests;
using InfraFlowSculptor.Contracts.StorageAccounts.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class StorageAccountController
{
    public static IApplicationBuilder UseStorageAccountController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var storageAccounts = endpoints.MapGroup("/storage-accounts")
                .WithTags("StorageAccounts");

            storageAccounts.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetStorageAccountQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            sa =>
                            {
                                var response = mapper.Map<StorageAccountResponse>(sa);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetStorageAccount")
                .WithSummary("Get a Storage Account")
                .WithDescription("Returns the full details of a single Azure Storage Account, including its Blob Containers, Queues, and Tables.")
                .Produces<StorageAccountResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            storageAccounts.MapPost("",
                    async (CreateStorageAccountRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateStorageAccountCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sa =>
                            {
                                var response = mapper.Map<StorageAccountResponse>(sa);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetStorageAccount",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateStorageAccount")
                .WithSummary("Create a Storage Account")
                .WithDescription("Creates a new Azure Storage Account resource inside the specified Resource Group. Requires Owner or Contributor access.")
                .Produces<StorageAccountResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            storageAccounts.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateStorageAccountRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateStorageAccountCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            sa =>
                            {
                                var response = mapper.Map<StorageAccountResponse>(sa);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateStorageAccount")
                .WithSummary("Update a Storage Account")
                .WithDescription("Replaces all mutable properties of an existing Storage Account. Requires Owner or Contributor access.")
                .Produces<StorageAccountResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            storageAccounts.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteStorageAccountCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteStorageAccount")
                .WithSummary("Delete a Storage Account")
                .WithDescription("Permanently deletes an Azure Storage Account resource and all its sub-resources (Blob Containers, Queues, Tables). Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // Blob Containers
            storageAccounts.MapPost("/{id:guid}/blob-containers",
                    async ([FromRoute] Guid id, AddBlobContainerRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var publicAccess = new BlobContainerPublicAccess(
                            Enum.Parse<BlobContainerPublicAccess.AccessLevel>(request.PublicAccess));

                        var command = new AddBlobContainerCommand(new AzureResourceId(id), request.Name, publicAccess);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sa =>
                            {
                                var response = mapper.Map<StorageAccountResponse>(sa);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("AddBlobContainer")
                .WithSummary("Add a Blob Container")
                .WithDescription("Adds a new Blob Container to the specified Storage Account. Returns the updated Storage Account. Requires Owner or Contributor access.")
                .Produces<StorageAccountResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            storageAccounts.MapDelete("/{id:guid}/blob-containers/{containerId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid containerId, IMediator mediator) =>
                    {
                        var command = new RemoveBlobContainerCommand(
                            new AzureResourceId(id),
                            new BlobContainerId(containerId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveBlobContainer")
                .WithSummary("Remove a Blob Container")
                .WithDescription("Removes a Blob Container from the specified Storage Account. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            storageAccounts.MapPut("/{id:guid}/blob-containers/{containerId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid containerId, UpdateBlobContainerPublicAccessRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var publicAccess = new BlobContainerPublicAccess(
                            Enum.Parse<BlobContainerPublicAccess.AccessLevel>(request.PublicAccess));

                        var command = new UpdateBlobContainerPublicAccessCommand(
                            new AzureResourceId(id),
                            new BlobContainerId(containerId),
                            publicAccess);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sa =>
                            {
                                var response = mapper.Map<StorageAccountResponse>(sa);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateBlobContainerPublicAccess")
                .WithSummary("Update Blob Container Public Access")
                .WithDescription("Updates the public access level of an existing Blob Container. Returns the updated Storage Account. Requires Owner or Contributor access.")
                .Produces<StorageAccountResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // Queues
            storageAccounts.MapPost("/{id:guid}/queues",
                    async ([FromRoute] Guid id, AddQueueRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddQueueCommand(new AzureResourceId(id), request.Name);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sa =>
                            {
                                var response = mapper.Map<StorageAccountResponse>(sa);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("AddQueue")
                .WithSummary("Add a Storage Queue")
                .WithDescription("Adds a new Storage Queue to the specified Storage Account. Returns the updated Storage Account. Requires Owner or Contributor access.")
                .Produces<StorageAccountResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            storageAccounts.MapDelete("/{id:guid}/queues/{queueId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid queueId, IMediator mediator) =>
                    {
                        var command = new RemoveQueueCommand(
                            new AzureResourceId(id),
                            new StorageQueueId(queueId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveQueue")
                .WithSummary("Remove a Storage Queue")
                .WithDescription("Removes a Storage Queue from the specified Storage Account. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // Tables
            storageAccounts.MapPost("/{id:guid}/tables",
                    async ([FromRoute] Guid id, AddTableRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddTableCommand(new AzureResourceId(id), request.Name);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sa =>
                            {
                                var response = mapper.Map<StorageAccountResponse>(sa);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("AddTable")
                .WithSummary("Add a Storage Table")
                .WithDescription("Adds a new Storage Table to the specified Storage Account. Returns the updated Storage Account. Requires Owner or Contributor access.")
                .Produces<StorageAccountResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            storageAccounts.MapDelete("/{id:guid}/tables/{tableId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid tableId, IMediator mediator) =>
                    {
                        var command = new RemoveTableCommand(
                            new AzureResourceId(id),
                            new StorageTableId(tableId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveTable")
                .WithSummary("Remove a Storage Table")
                .WithDescription("Removes a Storage Table from the specified Storage Account. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
