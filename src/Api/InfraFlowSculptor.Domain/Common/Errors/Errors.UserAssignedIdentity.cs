using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>
    /// Error definitions for the <see cref="UserAssignedIdentityAggregate.UserAssignedIdentity"/> aggregate.
    /// </summary>
    public static class UserAssignedIdentity
    {
        /// <summary>
        /// Returns a not-found error for the specified user-assigned identity identifier.
        /// </summary>
        /// <param name="id">The identifier that was not found.</param>
        /// <returns>An <see cref="Error"/> with code <c>UserAssignedIdentity.NotFound</c>.</returns>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "UserAssignedIdentity.NotFound",
            description: $"A user-assigned identity with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
