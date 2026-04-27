using ErrorOr;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the base Azure resource operations.</summary>
    public static class AzureResource
    {
        /// <summary>Returned when attempting to modify deployment configuration of an existing (pre-deployed) resource.</summary>
        public static Error CannotModifyExistingResource() => Error.Conflict(
            code: "AzureResource.CannotModifyExistingResource",
            description: "Deployment configuration cannot be modified on an existing resource that is not managed by this project."
        );
    }
}
