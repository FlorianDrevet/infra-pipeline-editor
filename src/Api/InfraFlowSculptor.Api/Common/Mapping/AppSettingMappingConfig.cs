using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.AppSettings.Queries.CheckKeyVaultAccess;
using InfraFlowSculptor.Application.AppSettings.Queries.GetAvailableOutputs;
using InfraFlowSculptor.Contracts.AppSettings.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for app settings request/response types.</summary>
public sealed class AppSettingMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AppSettingId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<AppSettingResult, AppSettingResponse>()
            .MapWith(src => new AppSettingResponse(
                src.Id.Value.ToString(),
                src.ResourceId.Value.ToString(),
                src.Name,
                src.EnvironmentValues != null ? new Dictionary<string, string>(src.EnvironmentValues) : null,
                src.SourceResourceId != null ? src.SourceResourceId.Value.ToString() : null,
                src.SourceOutputName,
                src.IsOutputReference,
                src.KeyVaultResourceId != null ? src.KeyVaultResourceId.Value.ToString() : null,
                src.SecretName,
                src.IsKeyVaultReference,
                src.HasKeyVaultAccess,
                src.SecretValueAssignment != null ? src.SecretValueAssignment.ToString() : null,
                src.VariableGroupId != null ? src.VariableGroupId.Value.ToString() : null,
                src.PipelineVariableName,
                src.VariableGroupName,
                src.IsViaVariableGroup));

        config.NewConfig<CheckKeyVaultAccessResult, CheckKeyVaultAccessResponse>()
            .MapWith(src => new CheckKeyVaultAccessResponse(
                src.HasAccess,
                src.MissingRoleDefinitionId,
                src.MissingRoleName));

        config.NewConfig<AvailableOutputsResult, AvailableOutputsResponse>()
            .MapWith(src => new AvailableOutputsResponse(
                src.ResourceTypeName,
                src.Outputs.Select(o => new OutputDefinitionResponse(o.Name, o.Description, o.IsSensitive)).ToList()));
    }
}
