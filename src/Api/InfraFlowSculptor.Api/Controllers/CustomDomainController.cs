using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.CustomDomains.Commands.AddCustomDomain;
using InfraFlowSculptor.Application.CustomDomains.Commands.RemoveCustomDomain;
using InfraFlowSculptor.Application.CustomDomains.Queries.ListCustomDomains;
using InfraFlowSculptor.Contracts.CustomDomains.Requests;
using InfraFlowSculptor.Contracts.CustomDomains.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>API endpoints for managing custom domain bindings on compute resources.</summary>
public static class CustomDomainController
{
    /// <summary>Registers the custom domain endpoints.</summary>
    public static IApplicationBuilder UseCustomDomainController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/azure-resources/{resourceId:guid}/custom-domains")
                .WithTags("CustomDomains");

            group.MapGet("",
                    async ([FromRoute] Guid resourceId, IMediator mediator) =>
                    {
                        var query = new ListCustomDomainsQuery(new AzureResourceId(resourceId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            domains => Results.Ok(domains.Adapt<List<CustomDomainResponse>>()),
                            errors => errors.Result());
                    })
                .WithName("ListCustomDomains")
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPost("",
                    async ([FromRoute] Guid resourceId,
                        [FromBody] AddCustomDomainRequest request,
                        IMediator mediator) =>
                    {
                        var command = new AddCustomDomainCommand(
                            new AzureResourceId(resourceId),
                            request.EnvironmentName,
                            request.DomainName,
                            request.BindingType);

                        var result = await mediator.Send(command);

                        return result.Match(
                            domain => Results.Created(
                                $"/azure-resources/{resourceId}/custom-domains/{domain.Id.Value}",
                                domain.Adapt<CustomDomainResponse>()),
                            errors => errors.Result());
                    })
                .WithName("AddCustomDomain")
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{customDomainId:guid}",
                    async ([FromRoute] Guid resourceId,
                        [FromRoute] Guid customDomainId,
                        IMediator mediator) =>
                    {
                        var command = new RemoveCustomDomainCommand(
                            new AzureResourceId(resourceId),
                            new CustomDomainId(customDomainId));

                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result());
                    })
                .WithName("RemoveCustomDomain")
                .ProducesProblem(StatusCodes.Status401Unauthorized);
        });
    }
}
