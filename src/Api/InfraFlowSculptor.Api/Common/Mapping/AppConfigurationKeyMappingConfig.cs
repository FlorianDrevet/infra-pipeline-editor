using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Contracts.AppConfigurations.Responses;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for App Configuration key request/response types.</summary>
public sealed class AppConfigurationKeyMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AppConfigurationKeyId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<AppConfigurationKeyResult, AppConfigurationKeyResponse>()
            .MapWith(src => new AppConfigurationKeyResponse(
                src.Id.Value.ToString(),
                src.AppConfigurationId.Value.ToString(),
                src.Key,
                src.Label,
                src.EnvironmentValues != null ? new Dictionary<string, string>(src.EnvironmentValues) : null,
                src.KeyVaultResourceId != null ? src.KeyVaultResourceId.Value.ToString() : null,
                src.SecretName,
                src.IsKeyVaultReference,
                src.HasKeyVaultAccess,
                src.SecretValueAssignment != null ? src.SecretValueAssignment.ToString() : null,
                src.VariableGroupId != null ? src.VariableGroupId.Value.ToString() : null,
                src.PipelineVariableName,
                src.VariableGroupName,
                src.IsViaVariableGroup));
    }
}
