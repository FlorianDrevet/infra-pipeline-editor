using InfraFlowSculptor.Application.DocumentIntelligences.Commands.CreateDocumentIntelligence;
using InfraFlowSculptor.Application.DocumentIntelligences.Commands.DeleteDocumentIntelligence;
using InfraFlowSculptor.Application.DocumentIntelligences.Commands.UpdateDocumentIntelligence;
using InfraFlowSculptor.Application.DocumentIntelligences.Queries;
using InfraFlowSculptor.Contracts.DocumentIntelligences.Requests;
using InfraFlowSculptor.Contracts.DocumentIntelligences.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Api.Controllers.Constants;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the Document Intelligence resource.</summary>
public static class DocumentIntelligenceController
{
    /// <summary>Registers the Document Intelligence endpoints under <c>/document-intelligence</c>.</summary>
    public static IApplicationBuilder UseDocumentIntelligenceController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.DocumentIntelligence)
                .WithTags("Document Intelligence");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetDocumentIntelligenceQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            di =>
                            {
                                var response = mapper.Map<DocumentIntelligenceResponse>(di);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(DocumentIntelligenceRouteNames.GetDocumentIntelligence)
                .WithSummary("Get a Document Intelligence resource")
                .WithDescription("Returns the full details of a single Azure Document Intelligence resource.")
                .Produces<DocumentIntelligenceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateDocumentIntelligenceRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateDocumentIntelligenceCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            di =>
                            {
                                var response = mapper.Map<DocumentIntelligenceResponse>(di);
                                return TypedResults.CreatedAtRoute(
                                    routeName: DocumentIntelligenceRouteNames.GetDocumentIntelligence,
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(DocumentIntelligenceRouteNames.CreateDocumentIntelligence)
                .WithSummary("Create a Document Intelligence resource")
                .WithDescription("Creates a new Azure Document Intelligence resource inside the specified Resource Group.")
                .Produces<DocumentIntelligenceResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateDocumentIntelligenceRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateDocumentIntelligenceCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            di =>
                            {
                                var response = mapper.Map<DocumentIntelligenceResponse>(di);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(DocumentIntelligenceRouteNames.UpdateDocumentIntelligence)
                .WithSummary("Update a Document Intelligence resource")
                .WithDescription("Replaces all mutable properties of an existing Document Intelligence resource.")
                .Produces<DocumentIntelligenceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteDocumentIntelligenceCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(DocumentIntelligenceRouteNames.DeleteDocumentIntelligence)
                .WithSummary("Delete a Document Intelligence resource")
                .WithDescription("Permanently deletes an Azure Document Intelligence resource.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
