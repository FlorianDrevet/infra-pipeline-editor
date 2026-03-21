using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="ProjectAggregate.Project"/> aggregate.</summary>
    public static class Project
    {
        private const string NotFoundCode = "Project.NotFound";
        private const string ForbiddenCode = "Project.Forbidden";
        private const string AlreadyMemberCode = "Project.AlreadyMember";
        private const string MemberNotFoundCode = "Project.MemberNotFound";
        private const string CannotRemoveOwnerCode = "Project.CannotRemoveOwner";
        private const string ConfigAlreadyAssignedCode = "Project.ConfigAlreadyAssigned";

        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError(ProjectId id) =>
            Error.NotFound(code: NotFoundCode, description: $"No Project with id {id} was found.");

        /// <summary>Returns a forbidden error when the caller lacks write access.</summary>
        public static Error ForbiddenError() =>
            Error.Forbidden(code: ForbiddenCode, description: "You do not have permission to modify this Project.");

        /// <summary>Returns a conflict error when the user is already a member.</summary>
        public static Error AlreadyMemberError(UserId userId) =>
            Error.Conflict(code: AlreadyMemberCode, description: $"User {userId} is already a member of this project.");

        /// <summary>Returns a not-found error when the member is not found in the project.</summary>
        public static Error MemberNotFoundError(UserId userId) =>
            Error.NotFound(code: MemberNotFoundCode, description: $"User {userId} is not a member of this project.");

        /// <summary>Returns a conflict error when trying to remove the last owner.</summary>
        public static Error CannotRemoveOwnerError() =>
            Error.Conflict(code: CannotRemoveOwnerCode, description: "Cannot remove the last owner of the project.");

        /// <summary>Returns a conflict error when the configuration is already assigned to a project.</summary>
        public static Error ConfigAlreadyAssignedError() =>
            Error.Conflict(code: ConfigAlreadyAssignedCode, description: "This configuration is already assigned to a project.");
    }
}
