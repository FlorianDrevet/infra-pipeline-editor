using ErrorOr;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to infrastructure configuration members.</summary>
    public static class Member
    {
        /// <summary>Returned when a member with the specified user identifier does not exist on the configuration.</summary>
        public static Error NotFoundError(UserId userId) => Error.NotFound(
            code: "Member.NotFound",
            description: $"No member with userId {userId} exists on this configuration."
        );

        /// <summary>Returned when the user is already a member of the configuration.</summary>
        public static Error AlreadyMemberError(UserId userId) => Error.Conflict(
            code: "Member.AlreadyMember",
            description: $"User {userId} is already a member of this configuration."
        );

        /// <summary>Returned when the caller does not have the Owner role required to manage members.</summary>
        public static Error ForbiddenError() => Error.Forbidden(
            code: "Member.Forbidden",
            description: "You do not have the required Owner role to manage members."
        );

        /// <summary>Returned when attempting to remove the owner of a configuration.</summary>
        public static Error CannotRemoveOwnerError() => Error.Conflict(
            code: "Member.CannotRemoveOwner",
            description: "The owner of a configuration cannot be removed."
        );
    }
}
