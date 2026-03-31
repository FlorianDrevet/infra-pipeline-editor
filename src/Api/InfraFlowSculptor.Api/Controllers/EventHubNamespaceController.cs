using InfraFlowSculptor.Application.EventHubNamespaces.Commands.AddEventHub;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.AddEventHubConsumerGroup;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.DeleteEventHubNamespace;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHub;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHubConsumerGroup;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.UpdateEventHubNamespace;
using InfraFlowSculptor.Application.EventHubNamespaces.Queries;
using InfraFlowSculptor.Contracts.EventHubNamespaces.Requests;
using InfraFlowSculptor.Contracts.EventHubNamespaces.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the Event Hub Namespace resource.</summary>
public static class EventHubNamespaceController
{
    /// <summary>Registers the Event Hub Namespace endpoints under <c>/event-hubs</c>.</summary>
    public static IApplicationBuilder UseEventHubNamespaceController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/event-hubs")
                .WithTags("Event Hubs");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetEventHubNamespaceQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            eh =>
                            {
                                var response = mapper.Map<EventHubNamespaceResponse>(eh);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetEventHubNamespace")
                .WithSummary("Get an Event Hub Namespace")
                .WithDescription("Returns the full details of a single Azure Event Hub Namespace resource.")
                .Produces<EventHubNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateEventHubNamespaceRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateEventHubNamespaceCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            eh =>
                            {
                                var response = mapper.Map<EventHubNamespaceResponse>(eh);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetEventHubNamespace",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateEventHubNamespace")
                .WithSummary("Create an Event Hub Namespace")
                .WithDescription("Creates a new Azure Event Hub Namespace resource inside the specified Resource Group.")
                .Produces<EventHubNamespaceResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateEventHubNamespaceRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateEventHubNamespaceCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            eh =>
                            {
                                var response = mapper.Map<EventHubNamespaceResponse>(eh);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateEventHubNamespace")
                .WithSummary("Update an Event Hub Namespace")
                .WithDescription("Replaces all mutable properties of an existing Event Hub Namespace.")
                .Produces<EventHubNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteEventHubNamespaceCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteEventHubNamespace")
                .WithSummary("Delete an Event Hub Namespace")
                .WithDescription("Permanently deletes an Azure Event Hub Namespace resource.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // Event Hub sub-resource endpoints
            group.MapPost("/{id:guid}/event-hubs",
                    async ([FromRoute] Guid id, [FromBody] AddEventHubRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddEventHubCommand(new AzureResourceId(id), request.Name);
                        var result = await mediator.Send(command);

                        return result.Match(
                            eh => TypedResults.Ok(mapper.Map<EventHubNamespaceResponse>(eh)),
                            errors => errors.Result()
                        );
                    })
                .WithName("AddEventHub")
                .WithSummary("Add an event hub")
                .WithDescription("Adds a new event hub to the Event Hub Namespace.")
                .Produces<EventHubNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapDelete("/{id:guid}/event-hubs/{eventHubId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid eventHubId, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new RemoveEventHubCommand(new AzureResourceId(id), eventHubId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            eh => TypedResults.Ok(mapper.Map<EventHubNamespaceResponse>(eh)),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveEventHub")
                .WithSummary("Remove an event hub")
                .WithDescription("Removes an event hub from the Event Hub Namespace.")
                .Produces<EventHubNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound);

            // Consumer group sub-resource endpoints
            group.MapPost("/{id:guid}/consumer-groups",
                    async ([FromRoute] Guid id, [FromBody] AddEventHubConsumerGroupRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddEventHubConsumerGroupCommand(new AzureResourceId(id), request.EventHubName, request.ConsumerGroupName);
                        var result = await mediator.Send(command);

                        return result.Match(
                            eh => TypedResults.Ok(mapper.Map<EventHubNamespaceResponse>(eh)),
                            errors => errors.Result()
                        );
                    })
                .WithName("AddEventHubConsumerGroup")
                .WithSummary("Add a consumer group")
                .WithDescription("Adds a new consumer group to the Event Hub Namespace.")
                .Produces<EventHubNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapDelete("/{id:guid}/consumer-groups/{consumerGroupId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid consumerGroupId, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new RemoveEventHubConsumerGroupCommand(new AzureResourceId(id), consumerGroupId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            eh => TypedResults.Ok(mapper.Map<EventHubNamespaceResponse>(eh)),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveEventHubConsumerGroup")
                .WithSummary("Remove a consumer group")
                .WithDescription("Removes a consumer group from the Event Hub Namespace.")
                .Produces<EventHubNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound);
        });
    }
}

/// <summary>Request body for adding an event hub to an Event Hub Namespace.</summary>
public class AddEventHubRequest
{
    /// <summary>The event hub name.</summary>
    public required string Name { get; init; }
}

/// <summary>Request body for adding a consumer group to an Event Hub Namespace.</summary>
public class AddEventHubConsumerGroupRequest
{
    /// <summary>The event hub name.</summary>
    public required string EventHubName { get; init; }

    /// <summary>The consumer group name within the event hub.</summary>
    public required string ConsumerGroupName { get; init; }
}
