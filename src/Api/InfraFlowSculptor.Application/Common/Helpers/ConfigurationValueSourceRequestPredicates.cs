using InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;
using InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Helpers;

internal static class ConfigurationValueSourceRequestPredicates
{
    internal static bool IsVariableGroupKeyVaultReference(AddAppSettingCommand request) =>
        IsVariableGroupKeyVaultReference(
            request.VariableGroupId,
            request.PipelineVariableName,
            request.KeyVaultResourceId,
            request.SecretName);

    internal static bool IsVariableGroupKeyVaultReference(AddAppConfigurationKeyCommand request) =>
        IsVariableGroupKeyVaultReference(
            request.VariableGroupId,
            request.PipelineVariableName,
            request.KeyVaultResourceId,
            request.SecretName);

    internal static bool IsVariableGroupReference(AddAppSettingCommand request) =>
        IsVariableGroupReference(request.VariableGroupId, request.PipelineVariableName);

    internal static bool IsVariableGroupReference(AddAppConfigurationKeyCommand request) =>
        IsVariableGroupReference(request.VariableGroupId, request.PipelineVariableName);

    internal static bool IsExportToKeyVault(AddAppSettingCommand request) =>
        IsExportToKeyVault(
            request.ExportToKeyVault,
            request.SourceResourceId,
            request.SourceOutputName,
            request.KeyVaultResourceId,
            request.SecretName);

    internal static bool IsExportToKeyVault(AddAppConfigurationKeyCommand request) =>
        IsExportToKeyVault(
            request.ExportToKeyVault,
            request.SourceResourceId,
            request.SourceOutputName,
            request.KeyVaultResourceId,
            request.SecretName);

    internal static bool IsKeyVaultReference(AddAppSettingCommand request) =>
        HasKeyVaultReference(request.KeyVaultResourceId, request.SecretName);

    internal static bool IsKeyVaultReference(AddAppConfigurationKeyCommand request) =>
        HasKeyVaultReference(request.KeyVaultResourceId, request.SecretName);

    internal static bool IsOutputReference(AddAppSettingCommand request) =>
        HasOutputReference(request.SourceResourceId, request.SourceOutputName);

    internal static bool IsOutputReference(AddAppConfigurationKeyCommand request) =>
        HasOutputReference(request.SourceResourceId, request.SourceOutputName);

    private static bool IsVariableGroupKeyVaultReference(
        Guid? variableGroupId,
        string? pipelineVariableName,
        AzureResourceId? keyVaultResourceId,
        string? secretName)
    {
        return IsVariableGroupReference(variableGroupId, pipelineVariableName)
            && HasKeyVaultReference(keyVaultResourceId, secretName);
    }

    private static bool IsVariableGroupReference(Guid? variableGroupId, string? pipelineVariableName)
    {
        return variableGroupId is not null
            && pipelineVariableName is not null;
    }

    private static bool IsExportToKeyVault(
        bool exportToKeyVault,
        AzureResourceId? sourceResourceId,
        string? sourceOutputName,
        AzureResourceId? keyVaultResourceId,
        string? secretName)
    {
        return exportToKeyVault
            && HasOutputReference(sourceResourceId, sourceOutputName)
            && HasKeyVaultReference(keyVaultResourceId, secretName);
    }

    private static bool HasKeyVaultReference(AzureResourceId? keyVaultResourceId, string? secretName)
    {
        return keyVaultResourceId is not null
            && secretName is not null;
    }

    private static bool HasOutputReference(AzureResourceId? sourceResourceId, string? sourceOutputName)
    {
        return sourceResourceId is not null
            && sourceOutputName is not null;
    }
}