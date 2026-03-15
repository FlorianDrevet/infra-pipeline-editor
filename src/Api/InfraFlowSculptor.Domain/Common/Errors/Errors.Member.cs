using ErrorOr;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class Member
    {
        public static Error NotFoundError(UserId userId) => Error.NotFound(
            code: "Member.NotFound",
            description: $"No member with userId {userId} exists on this configuration."
        );

        public static Error AlreadyMemberError(UserId userId) => Error.Conflict(
            code: "Member.AlreadyMember",
            description: $"User {userId} is already a member of this configuration."
        );

        public static Error ForbiddenError() => Error.Forbidden(
            code: "Member.Forbidden",
            description: "You do not have the required Owner role to manage members."
        );

        public static Error CannotRemoveOwnerError() => Error.Conflict(
            code: "Member.CannotRemoveOwner",
            description: "The owner of a configuration cannot be removed."
        );
    }
}
