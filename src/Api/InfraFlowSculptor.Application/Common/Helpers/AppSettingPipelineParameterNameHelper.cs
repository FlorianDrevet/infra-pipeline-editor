using System.Text;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Helpers;

/// <summary>
/// Resolves the Bicep parameter name to use for pipeline variable group mappings derived from app settings.
/// </summary>
internal static class AppSettingPipelineParameterNameHelper
{
    private const string DefaultResourceIdentifier = "resource";
    private const string SecureParameterSuffix = "SecretValue";

    /// <summary>
    /// Resolves the generated Bicep parameter name for the provided app setting.
    /// </summary>
    /// <param name="appSetting">The app setting mapped into a pipeline variable group.</param>
    /// <returns>The Bicep parameter name expected by the generated template.</returns>
    internal static string ResolveBicepParameterName(AppSettingReadModel appSetting)
    {
        ArgumentNullException.ThrowIfNull(appSetting);

        if (!RequiresSecureKeyVaultParameter(appSetting))
            return appSetting.Name;

        return BuildSecureAppSettingParameterName(appSetting.ResourceName, appSetting.SecretName!);
    }

    private static bool RequiresSecureKeyVaultParameter(AppSettingReadModel appSetting)
    {
        return appSetting.IsViaVariableGroup
            && appSetting.IsKeyVaultReference
            && !string.IsNullOrWhiteSpace(appSetting.SecretName)
            && string.Equals(
                appSetting.SecretValueAssignment,
                nameof(SecretValueAssignment.ViaBicepparam),
                StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildSecureAppSettingParameterName(string targetResourceName, string secretName)
    {
        var moduleName = ToBicepIdentifier(targetResourceName);
        var pascalSecretName = ToPascalCaseFromEnvVar(secretName);
        return $"{moduleName}{pascalSecretName}{SecureParameterSuffix}";
    }

    private static string ToBicepIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DefaultResourceIdentifier;

        var parts = name.Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return DefaultResourceIdentifier;

        var stringBuilder = new StringBuilder(parts[0].ToLowerInvariant());
        foreach (var part in parts.Skip(1))
        {
            if (part.Length == 0)
                continue;

            stringBuilder
                .Append(char.ToUpperInvariant(part[0]))
                .Append(part[1..].ToLowerInvariant());
        }

        return stringBuilder.ToString();
    }

    private static string ToPascalCaseFromEnvVar(string envVarName)
    {
        var stringBuilder = new StringBuilder();
        var capitalizeNext = true;

        foreach (var character in envVarName)
        {
            if (character is '_' or '-' or '.')
            {
                capitalizeNext = true;
                continue;
            }

            stringBuilder.Append(capitalizeNext
                ? char.ToUpperInvariant(character)
                : char.ToLowerInvariant(character));

            capitalizeNext = false;
        }

        return stringBuilder.ToString();
    }
}