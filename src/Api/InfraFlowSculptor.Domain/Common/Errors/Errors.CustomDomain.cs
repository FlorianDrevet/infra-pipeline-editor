using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to custom domain bindings.</summary>
    public static class CustomDomain
    {
        private const string DuplicateDomainCode = "CustomDomain.DuplicateDomain";
        private const string ResourceNotFoundCode = "CustomDomain.ResourceNotFound";
        private const string NotFoundCode = "CustomDomain.NotFound";
        private const string NotSupportedCode = "CustomDomain.NotSupportedForResourceType";
        private const string NotSupportedExistingCode = "CustomDomain.NotSupportedForExistingResource";

        /// <summary>Returned when a custom domain with the same name already exists for the given environment.</summary>
        public static Error DuplicateDomain(string environmentName, string domainName) => Error.Conflict(
            code: DuplicateDomainCode,
            description: $"The domain '{domainName}' is already configured for environment '{environmentName}'.");

        /// <summary>Returned when the specified Azure resource does not exist.</summary>
        public static Error ResourceNotFound(AzureResourceId id) => Error.NotFound(
            code: ResourceNotFoundCode,
            description: $"No Azure resource with id '{id}' was found.");

        /// <summary>Returned when a custom domain with the specified identifier does not exist.</summary>
        public static Error NotFound(CustomDomainId id) => Error.NotFound(
            code: NotFoundCode,
            description: $"No custom domain with id '{id}' was found.");

        /// <summary>Returned when custom domains are not supported for the given resource type.</summary>
        public static Error NotSupportedForResourceType(string resourceType) => Error.Validation(
            code: NotSupportedCode,
            description: $"Custom domains are not supported for resource type '{resourceType}'.");

        /// <summary>Returned when custom domains cannot be added to an existing (non-managed) resource.</summary>
        public static Error NotSupportedForExistingResource() => Error.Validation(
            code: NotSupportedExistingCode,
            description: "Custom domains cannot be configured on an existing (non-managed) resource.");
    }
}
