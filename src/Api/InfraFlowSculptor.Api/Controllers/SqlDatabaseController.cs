using InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;
using InfraFlowSculptor.Application.SqlDatabases.Commands.DeleteSqlDatabase;
using InfraFlowSculptor.Application.SqlDatabases.Commands.UpdateSqlDatabase;
using InfraFlowSculptor.Application.SqlDatabases.Queries;
using MediatR;
using InfraFlowSculptor.Contracts.SqlDatabases.Requests;
using InfraFlowSculptor.Contracts.SqlDatabases.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for the SQL Database feature.</summary>
public static class SqlDatabaseController
{
    /// <summary>Registers the SQL Database endpoints on the application builder.</summary>
    public static IApplicationBuilder UseSqlDatabaseController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/sql-database")
                .WithTags("SQL Databases");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetSqlDatabaseQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            database =>
                            {
                                var response = mapper.Map<SqlDatabaseResponse>(database);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetSqlDatabase")
                .WithSummary("Get a SQL Database")
                .WithDescription("Returns the full details of a single Azure SQL Database resource.")
                .Produces<SqlDatabaseResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateSqlDatabaseRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateSqlDatabaseCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            database =>
                            {
                                var response = mapper.Map<SqlDatabaseResponse>(database);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetSqlDatabase",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateSqlDatabase")
                .WithSummary("Create a SQL Database")
                .WithDescription("Creates a new Azure SQL Database resource inside the specified Resource Group. The SQL Server must already exist. Requires Owner or Contributor access.")
                .Produces<SqlDatabaseResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateSqlDatabaseRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateSqlDatabaseCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            database =>
                            {
                                var response = mapper.Map<SqlDatabaseResponse>(database);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateSqlDatabase")
                .WithSummary("Update a SQL Database")
                .WithDescription("Replaces all mutable properties of an existing SQL Database. Requires Owner or Contributor access.")
                .Produces<SqlDatabaseResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteSqlDatabaseCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteSqlDatabase")
                .WithSummary("Delete a SQL Database")
                .WithDescription("Permanently deletes an Azure SQL Database resource. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
