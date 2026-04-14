using ErrorOr;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the Resource Group aggregate.</summary>
    public static class ResourceGroup
    {
        /// <summary>Well-known error codes for Resource Group operations.</summary>
        public static class Codes
        {
            /// <summary>Error code: resource group not found.</summary>
            public const string NotFoundCode = "ResourceGroup.NotFound";

            /// <summary>Error code: resource group already exists.</summary>
            public const string AlreadyExistsCode = "ResourceGroup.AlreadyExists";
        }

        /// <summary>Returned when a resource group with the specified identifier does not exist.</summary>
        public static Error NotFound(ResourceGroupId id) => Error.NotFound(
            code: Codes.NotFoundCode,
            description: $"Resource group not found with id {id}.",
            metadata: new Dictionary<string, object> {{"id", id.ToString()}}
        );

        /// <summary>Returned when a resource group already exists.</summary>
        public static Error AlreadyExists() => Error.Validation(
            code: Codes.AlreadyExistsCode,
            description: "Resource group already exists."
        );

        /// <summary>Errors related to adding a resource to a resource group.</summary>
        public static class AddResource
        {
            /// <summary>Well-known error codes for add-resource operations.</summary>
            public static class ErrorCodes
            {
                /// <summary>Error code: resource group resource limit reached.</summary>
                public const string ResourceGroupResourceLimitReachedCode = "ResourceGroup.AddResource.ResourceLimitReached";

                /// <summary>Error code: resource already present in the group.</summary>
                public const string ResourceAlreadyInGroupCode = "ResourceGroup.AddResource.ResourceAlreadyInGroup";

                /// <summary>Error code: resource not in the same location as the group.</summary>
                public const string ResourceNotInSameLocationCode = "ResourceGroup.AddResource.ResourceNotInSameLocation";
            }

            /// <summary>Returned when the resource group has reached its maximum resource count.</summary>
            public static Error ResourceGroupResourceLimitReached() => Error.Validation(
                code: ErrorCodes.ResourceGroupResourceLimitReachedCode,
                description: "Resource group has reached the maximum number of resources allowed."
            );

            /// <summary>Returned when the resource is already present in the group.</summary>
            public static Error ResourceAlreadyInGroup() => Error.Validation(
                code: ErrorCodes.ResourceAlreadyInGroupCode,
                description: "The resource is already in the group."
            );

            /// <summary>Returned when the resource location does not match the resource group location.</summary>
            public static Error ResourceNotInSameLocation() => Error.Validation(
                code: ErrorCodes.ResourceNotInSameLocationCode,
                description: "The resource must be in the same location as the resource group."
            );
        }

        /// <summary>Errors related to removing a resource from a resource group.</summary>
        public static class RemoveResource
        {
            /// <summary>Well-known error codes for remove-resource operations.</summary>
            public static class ErrorCodes
            {
                /// <summary>Error code: resource not present in the group.</summary>
                public const string ResourceNotInGroupCode = "ResourceGroup.RemoveResource.ResourceNotInGroup";

                /// <summary>Error code: resource is a dependency for other resources.</summary>
                public const string ResourceIsDependencyCode = "ResourceGroup.RemoveResource.ResourceIsDependency";
            }

            /// <summary>Returned when the resource is not in the group.</summary>
            public static Error ResourceNotInGroup() => Error.Validation(
                code: ErrorCodes.ResourceNotInGroupCode,
                description: "The resource is not in the group."
            );

            /// <summary>Returned when the resource is a dependency for other resources and cannot be removed.</summary>
            public static Error ResourceIsDependency() => Error.Validation(
                code: ErrorCodes.ResourceIsDependencyCode,
                description: "The resource is a dependency for other resources in the group and cannot be removed."
            );
        }
    }
}