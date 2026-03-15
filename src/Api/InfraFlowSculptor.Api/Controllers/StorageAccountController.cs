using InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddQueue;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Commands.DeleteStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveBlobContainer;
using InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveQueue;
using InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveTable;
using InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Queries;
using InfraFlowSculptor.Contracts.StorageAccounts.Requests;
using InfraFlowSculptor.Contracts.StorageAccounts.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Errors;

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
                .WithName("GetStorageAccount");

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
                .WithName("CreateStorageAccount");

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
                .WithName("UpdateStorageAccount");

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
                .WithName("DeleteStorageAccount");

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
                .WithName("AddBlobContainer");

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
                .WithName("RemoveBlobContainer");

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
                .WithName("AddQueue");

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
                .WithName("RemoveQueue");

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
                .WithName("AddTable");

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
                .WithName("RemoveTable");
        });
    }
}
