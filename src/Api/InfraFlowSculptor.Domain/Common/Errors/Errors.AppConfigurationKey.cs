using ErrorOr;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for <see cref="AppConfigurationAggregate.Entities.AppConfigurationKey"/> operations.</summary>
    public static class AppConfigurationKey
    {
        /// <summary>Returns an error when the specified configuration key is not found.</summary>
        public static Error NotFoundError(AppConfigurationKeyId id) =>
            Error.NotFound(
                code: "AppConfigurationKey.NotFound",
                description: $"No configuration key with id '{id.Value}' was found.");

        /// <summary>Returns an error when the parent App Configuration resource is not found.</summary>
        public static Error AppConfigurationNotFound(AzureResourceId id) =>
            Error.NotFound(
                code: "AppConfigurationKey.AppConfigurationNotFound",
                description: $"The App Configuration resource with id '{id.Value}' was not found.");

        /// <summary>Returns an error when a configuration key with the same name already exists.</summary>
        public static Error DuplicateKeyError(string key) =>
            Error.Conflict(
                code: "AppConfigurationKey.DuplicateKey",
                description: $"A configuration key named '{key}' already exists on this App Configuration.");

        /// <summary>Returns an error when the Key Vault resource for a KV reference is not found.</summary>
        public static Error KeyVaultNotFound(AzureResourceId id) =>
            Error.NotFound(
                code: "AppConfigurationKey.KeyVaultNotFound",
                description: $"The Key Vault resource with id '{id.Value}' was not found.");

        /// <summary>Returns an error when the source resource for an output reference is not found.</summary>
        public static Error SourceResourceNotFound(AzureResourceId id) =>
            Error.NotFound(
                code: "AppConfigurationKey.SourceResourceNotFound",
                description: $"The source resource with id '{id.Value}' was not found.");

        /// <summary>Returns an error when the specified output is not valid for the source resource type.</summary>
        public static Error InvalidOutput(string outputName, string resourceType) =>
            Error.Validation(
                code: "AppConfigurationKey.InvalidOutput",
                description: $"Output '{outputName}' is not a valid output for resource type '{resourceType}'.");
    }
}
