using InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;
using InfraFlowSculptor.Application.AppSettings.Commands.RemoveAppSetting;
using InfraFlowSculptor.Application.AppSettings.Queries.CheckKeyVaultAccess;
using InfraFlowSculptor.Application.AppSettings.Queries.GetAvailableOutputs;
using InfraFlowSculptor.Application.AppSettings.Queries.ListAppSettings;
using InfraFlowSculptor.Contracts.AppSettings.Requests;
using InfraFlowSculptor.Contracts.AppSettings.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>API endpoints for managing app settings (environment variables) on compute resources.</summary>
public static class AppSettingController
{
    /// <summary>Registers the app settings endpoints.</summary>
    public static IApplicationBuilder UseAppSettingController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/azure-resources/{resourceId:guid}/app-settings")
                .WithTags("AppSettings");

            group.MapGet("",
                    async ([FromRoute] Guid resourceId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListAppSettingsQuery(new AzureResourceId(resourceId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            settings => Results.Ok(
                                settings.Select(s => mapper.Map<AppSettingResponse>(s)).ToList()),
                            errors => errors.Result());
                    })
                .WithName("ListAppSettings");

            group.MapPost("",
                    async ([FromRoute] Guid resourceId,
                        [FromBody] AddAppSettingRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = new AddAppSettingCommand(
                            new AzureResourceId(resourceId),
                            request.Name,
                            request.EnvironmentValues,
                            request.SourceResourceId.HasValue
                                ? new AzureResourceId(request.SourceResourceId.Value)
                                : null,
                            request.SourceOutputName,
                            request.KeyVaultResourceId.HasValue
                                ? new AzureResourceId(request.KeyVaultResourceId.Value)
                                : null,
                            request.SecretName,
                            request.ExportToKeyVault,
                            request.SecretValueAssignment is not null
                                ? Enum.Parse<SecretValueAssignment>(request.SecretValueAssignment, true)
                                : null,
                            request.VariableGroupId,
                            request.PipelineVariableName);

                        var result = await mediator.Send(command);

                        return result.Match(
                            appSetting => Results.Created(
                                $"/azure-resources/{resourceId}/app-settings/{appSetting.Id.Value}",
                                mapper.Map<AppSettingResponse>(appSetting)),
                            errors => errors.Result());
                    })
                .WithName("AddAppSetting");

            group.MapDelete("/{appSettingId:guid}",
                    async ([FromRoute] Guid resourceId,
                        [FromRoute] Guid appSettingId,
                        IMediator mediator) =>
                    {
                        var command = new RemoveAppSettingCommand(
                            new AzureResourceId(resourceId),
                            new AppSettingId(appSettingId));

                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result());
                    })
                .WithName("RemoveAppSetting");

            // Endpoint to get available outputs from a resource (for building the UI picker)
            var outputsGroup = endpoints.MapGroup("/azure-resources/{resourceId:guid}/available-outputs")
                .WithTags("AppSettings");

            outputsGroup.MapGet("",
                    async ([FromRoute] Guid resourceId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetAvailableOutputsQuery(new AzureResourceId(resourceId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            outputs => Results.Ok(mapper.Map<AvailableOutputsResponse>(outputs)),
                            errors => errors.Result());
                    })
                .WithName("GetAvailableOutputs");

            // Endpoint to check whether a compute resource has Key Vault access
            var kvAccessGroup = endpoints.MapGroup("/azure-resources/{resourceId:guid}/check-keyvault-access")
                .WithTags("AppSettings");

            kvAccessGroup.MapGet("/{keyVaultId:guid}",
                    async ([FromRoute] Guid resourceId, [FromRoute] Guid keyVaultId,
                        IMediator mediator, IMapper mapper) =>
                    {
                        var query = new CheckKeyVaultAccessQuery(
                            new AzureResourceId(resourceId),
                            new AzureResourceId(keyVaultId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            access => Results.Ok(mapper.Map<CheckKeyVaultAccessResponse>(access)),
                            errors => errors.Result());
                    })
                .WithName("CheckKeyVaultAccess");
        });
    }
}
