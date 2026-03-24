using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for <see cref="BaseModels.Entites.AppSetting"/> operations.</summary>
    public static class AppSetting
    {
        /// <summary>Returns an error when the specified app setting is not found.</summary>
        public static Error NotFoundError(AppSettingId id) =>
            Error.NotFound(
                code: "AppSetting.NotFound",
                description: $"No app setting with id '{id.Value}' was found.");

        /// <summary>Returns an error when the source resource for an output reference is not found.</summary>
        public static Error SourceResourceNotFound(AzureResourceId id) =>
            Error.NotFound(
                code: "AppSetting.SourceResourceNotFound",
                description: $"The source resource with id '{id.Value}' was not found.");

        /// <summary>Returns an error when the specified output is not valid for the source resource type.</summary>
        public static Error InvalidOutput(string outputName, string resourceType) =>
            Error.Validation(
                code: "AppSetting.InvalidOutput",
                description: $"The output '{outputName}' is not available on resource type '{resourceType}'.");

        /// <summary>Returns an error when app settings are not supported on the resource type.</summary>
        public static Error NotSupportedForResourceType(string resourceType) =>
            Error.Validation(
                code: "AppSetting.NotSupportedForResourceType",
                description: $"App settings (environment variables) are not supported on resource type '{resourceType}'. " +
                             "They can only be configured on WebApp, FunctionApp, and ContainerApp resources.");

        /// <summary>Returns an error when the Key Vault resource for a KV reference is not found.</summary>
        public static Error KeyVaultNotFound(AzureResourceId id) =>
            Error.NotFound(
                code: "AppSetting.KeyVaultNotFound",
                description: $"The Key Vault resource with id '{id.Value}' was not found.");
    }
}
