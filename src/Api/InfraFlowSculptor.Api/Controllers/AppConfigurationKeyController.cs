using InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;
using InfraFlowSculptor.Application.AppConfigurations.Commands.RemoveAppConfigurationKey;
using InfraFlowSculptor.Application.AppConfigurations.Queries.ListAppConfigurationKeys;
using InfraFlowSculptor.Contracts.AppConfigurations.Requests;
using InfraFlowSculptor.Contracts.AppConfigurations.Responses;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>API endpoints for managing configuration keys on App Configuration resources.</summary>
public static class AppConfigurationKeyController
{
    /// <summary>Registers the App Configuration key endpoints.</summary>
    public static IApplicationBuilder UseAppConfigurationKeyController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/azure-resources/{appConfigurationId:guid}/configuration-keys")
                .WithTags("AppConfigurationKeys");

            group.MapGet("",
                    async ([FromRoute] Guid appConfigurationId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListAppConfigurationKeysQuery(new AzureResourceId(appConfigurationId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            keys => Results.Ok(
                                keys.Select(k => mapper.Map<AppConfigurationKeyResponse>(k)).ToList()),
                            errors => errors.Result());
                    })
                .WithName("ListAppConfigurationKeys");

            group.MapPost("",
                    async ([FromRoute] Guid appConfigurationId,
                        [FromBody] AddAppConfigurationKeyRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = new AddAppConfigurationKeyCommand(
                            new AzureResourceId(appConfigurationId),
                            request.Key,
                            request.Label,
                            request.EnvironmentValues,
                            request.KeyVaultResourceId.HasValue
                                ? new AzureResourceId(request.KeyVaultResourceId.Value)
                                : null,
                            request.SecretName,
                            request.SecretValueAssignment is not null
                                ? Enum.Parse<SecretValueAssignment>(request.SecretValueAssignment, true)
                                : null,
                            request.VariableGroupId,
                            request.PipelineVariableName);

                        var result = await mediator.Send(command);

                        return result.Match(
                            configKey => Results.Created(
                                $"/azure-resources/{appConfigurationId}/configuration-keys/{configKey.Id.Value}",
                                mapper.Map<AppConfigurationKeyResponse>(configKey)),
                            errors => errors.Result());
                    })
                .WithName("AddAppConfigurationKey");

            group.MapDelete("/{configurationKeyId:guid}",
                    async ([FromRoute] Guid appConfigurationId,
                        [FromRoute] Guid configurationKeyId,
                        IMediator mediator) =>
                    {
                        var command = new RemoveAppConfigurationKeyCommand(
                            new AzureResourceId(appConfigurationId),
                            new AppConfigurationKeyId(configurationKeyId));

                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result());
                    })
                .WithName("RemoveAppConfigurationKey");
        });
    }
}
