using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class RoleAssignment
    {
        public static Error SourceResourceNotFound(AzureResourceId id) => Error.NotFound(
            code: "RoleAssignment.SourceResourceNotFound",
            description: $"A resource with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        public static Error TargetResourceNotFound(AzureResourceId id) => Error.NotFound(
            code: "RoleAssignment.TargetResourceNotFound",
            description: $"The target resource with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        public static Error NotFound(RoleAssignmentId id) => Error.NotFound(
            code: "RoleAssignment.NotFound",
            description: $"A role assignment with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
