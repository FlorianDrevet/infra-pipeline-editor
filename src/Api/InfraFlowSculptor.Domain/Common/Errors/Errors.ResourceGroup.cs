using ErrorOr;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class ResourceGroup
    {
        public static class Codes
        {
            public const string NotFoundCode = "ResourceGroup.NotFound";
            public const string AlreadyExistsCode = "ResourceGroup.AlreadyExists";
        }
        
        public static Error NotFound(ResourceGroupId id) => Error.NotFound(
            code: Codes.NotFoundCode,
            description: $"Resource group not found with id {id}.",
            metadata: new Dictionary<string, object> {{"id", id.ToString()}}
        );

        public static Error AlreadyExists() => Error.Validation(
            code: Codes.AlreadyExistsCode,
            description: "Resource group already exists."
        );

        public static class AddResource
        {
            public static class Codes
            {
                public const string ResourceGroupResourceLimitReached = "ResourceGroup.AddResource.ResourceLimitReached";
                public const string ResourceAlreadyInGroupCode = "ResourceGroup.AddResource.ResourceAlreadyInGroup";
                public const string ResourceNotInSameLocationCode = "ResourceGroup.AddResource.ResourceNotInSameLocation";
            }


            public static Error ResourceGroupResourceLimitReached() => Error.Validation(
                code: Codes.ResourceGroupResourceLimitReached,
                description: "Resource group has reached the maximum number of resources allowed."
            );

            public static Error ResourceAlreadyInGroup() => Error.Validation(
                code: Codes.ResourceAlreadyInGroupCode,
                description: "The resource is already in the group."
            );
            
            public static Error ResourceNotInSameLocation() => Error.Validation(
                code: Codes.ResourceNotInSameLocationCode,
                description: "The resource must be in the same location as the resource group."
            );
        }
        
        public static class RemoveResource
        {
            public static class Codes
            {
                public const string ResourceNotInGroupCode = "ResourceGroup.RemoveResource.ResourceNotInGroup";
                public const string ResourceIsDependencyCode = "ResourceGroup.RemoveResource.ResourceIsDependency";
            }
            
            public static Error ResourceNotInGroup() => Error.Validation(
                code: Codes.ResourceNotInGroupCode,
                description: "The resource is not in the group."
            );
            
            public static Error ResourceIsDependency() => Error.Validation(
                code: Codes.ResourceIsDependencyCode,
                description: "The resource is a dependency for other resources in the group and cannot be removed."
            );
        }
    }
}