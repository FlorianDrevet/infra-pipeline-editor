using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusQueue;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusTopicSubscription;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.DeleteServiceBusNamespace;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.RemoveServiceBusQueue;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.RemoveServiceBusTopicSubscription;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.UpdateServiceBusNamespace;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Queries;
using InfraFlowSculptor.Contracts.ServiceBusNamespaces.Requests;
using InfraFlowSculptor.Contracts.ServiceBusNamespaces.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the Service Bus Namespace resource.</summary>
public static class ServiceBusNamespaceController
{
    /// <summary>Registers the Service Bus Namespace endpoints under <c>/service-bus</c>.</summary>
    public static IApplicationBuilder UseServiceBusNamespaceController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/service-bus")
                .WithTags("Service Bus");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetServiceBusNamespaceQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            sb =>
                            {
                                var response = mapper.Map<ServiceBusNamespaceResponse>(sb);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetServiceBusNamespace")
                .WithSummary("Get a Service Bus Namespace")
                .WithDescription("Returns the full details of a single Azure Service Bus Namespace resource.")
                .Produces<ServiceBusNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateServiceBusNamespaceRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateServiceBusNamespaceCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sb =>
                            {
                                var response = mapper.Map<ServiceBusNamespaceResponse>(sb);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetServiceBusNamespace",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateServiceBusNamespace")
                .WithSummary("Create a Service Bus Namespace")
                .WithDescription("Creates a new Azure Service Bus Namespace resource inside the specified Resource Group.")
                .Produces<ServiceBusNamespaceResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateServiceBusNamespaceRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateServiceBusNamespaceCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            sb =>
                            {
                                var response = mapper.Map<ServiceBusNamespaceResponse>(sb);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateServiceBusNamespace")
                .WithSummary("Update a Service Bus Namespace")
                .WithDescription("Replaces all mutable properties of an existing Service Bus Namespace.")
                .Produces<ServiceBusNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteServiceBusNamespaceCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteServiceBusNamespace")
                .WithSummary("Delete a Service Bus Namespace")
                .WithDescription("Permanently deletes an Azure Service Bus Namespace resource.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // Queue sub-resource endpoints
            group.MapPost("/{id:guid}/queues",
                    async ([FromRoute] Guid id, [FromBody] AddServiceBusQueueRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddServiceBusQueueCommand(new AzureResourceId(id), request.Name);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sb => TypedResults.Ok(mapper.Map<ServiceBusNamespaceResponse>(sb)),
                            errors => errors.Result()
                        );
                    })
                .WithName("AddServiceBusQueue")
                .WithSummary("Add a queue")
                .WithDescription("Adds a new queue to the Service Bus Namespace.")
                .Produces<ServiceBusNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapDelete("/{id:guid}/queues/{queueId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid queueId, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new RemoveServiceBusQueueCommand(new AzureResourceId(id), queueId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sb => TypedResults.Ok(mapper.Map<ServiceBusNamespaceResponse>(sb)),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveServiceBusQueue")
                .WithSummary("Remove a queue")
                .WithDescription("Removes a queue from the Service Bus Namespace.")
                .Produces<ServiceBusNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            // Topic subscription sub-resource endpoints
            group.MapPost("/{id:guid}/topic-subscriptions",
                    async ([FromRoute] Guid id, [FromBody] AddServiceBusTopicSubscriptionRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddServiceBusTopicSubscriptionCommand(new AzureResourceId(id), request.TopicName, request.SubscriptionName);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sb => TypedResults.Ok(mapper.Map<ServiceBusNamespaceResponse>(sb)),
                            errors => errors.Result()
                        );
                    })
                .WithName("AddServiceBusTopicSubscription")
                .WithSummary("Add a topic subscription")
                .WithDescription("Adds a new topic subscription to the Service Bus Namespace.")
                .Produces<ServiceBusNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapDelete("/{id:guid}/topic-subscriptions/{subscriptionId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid subscriptionId, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new RemoveServiceBusTopicSubscriptionCommand(new AzureResourceId(id), subscriptionId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            sb => TypedResults.Ok(mapper.Map<ServiceBusNamespaceResponse>(sb)),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveServiceBusTopicSubscription")
                .WithSummary("Remove a topic subscription")
                .WithDescription("Removes a topic subscription from the Service Bus Namespace.")
                .Produces<ServiceBusNamespaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);
        });
    }
}

/// <summary>Request body for adding a queue to a Service Bus Namespace.</summary>
public class AddServiceBusQueueRequest
{
    /// <summary>The queue name.</summary>
    public required string Name { get; init; }
}

/// <summary>Request body for adding a topic subscription to a Service Bus Namespace.</summary>
public class AddServiceBusTopicSubscriptionRequest
{
    /// <summary>The topic name.</summary>
    public required string TopicName { get; init; }

    /// <summary>The subscription name within the topic.</summary>
    public required string SubscriptionName { get; init; }
}
