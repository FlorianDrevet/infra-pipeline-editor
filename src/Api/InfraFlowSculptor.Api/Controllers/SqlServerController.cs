using InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;
using InfraFlowSculptor.Application.SqlServers.Commands.DeleteSqlServer;
using InfraFlowSculptor.Application.SqlServers.Commands.UpdateSqlServer;
using InfraFlowSculptor.Application.SqlServers.Queries;
using InfraFlowSculptor.Application.Common.Queries.GetDependentResources;
using MediatR;
using InfraFlowSculptor.Contracts.SqlServers.Requests;
using InfraFlowSculptor.Contracts.SqlServers.Responses;
using InfraFlowSculptor.Contracts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for the SQL Server feature.</summary>
public static class SqlServerController
{
    /// <summary>Registers the SQL Server endpoints on the application builder.</summary>
    public static IApplicationBuilder UseSqlServerController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/sql-server")
                .WithTags("SQL Servers");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetSqlServerQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            server =>
                            {
                                var response = mapper.Map<SqlServerResponse>(server);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetSqlServer")
                .WithSummary("Get a SQL Server")
                .WithDescription("Returns the full details of a single Azure SQL Server resource.")
                .Produces<SqlServerResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateSqlServerRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateSqlServerCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            server =>
                            {
                                var response = mapper.Map<SqlServerResponse>(server);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetSqlServer",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateSqlServer")
                .WithSummary("Create a SQL Server")
                .WithDescription("Creates a new Azure SQL Server resource. Requires Owner or Contributor access.")
                .Produces<SqlServerResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateSqlServerRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateSqlServerCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            server =>
                            {
                                var response = mapper.Map<SqlServerResponse>(server);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateSqlServer")
                .WithSummary("Update a SQL Server")
                .WithDescription("Replaces all mutable properties of an existing SQL Server. Requires Owner or Contributor access.")
                .Produces<SqlServerResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteSqlServerCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteSqlServer")
                .WithSummary("Delete a SQL Server")
                .WithDescription("Permanently deletes an Azure SQL Server resource. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}/dependents",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var query = new GetDependentResourcesQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            dependents => Results.Ok(dependents.Select(d =>
                                new DependentResourceResponse(d.Id.ToString(), d.Name, d.ResourceType)).ToList()),
                            errors => errors.Result()
                        );
                    })
                .WithName("GetSqlServerDependents")
                .WithSummary("Get dependent resources")
                .WithDescription("Returns all resources that depend on this SQL Server and would be deleted alongside it.")
                .Produces<List<DependentResourceResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
