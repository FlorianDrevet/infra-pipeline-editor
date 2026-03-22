using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="ProjectAggregate.Project"/> aggregate.</summary>
    public static class Project
    {
        private const string NotFoundCode = "Project.NotFound";
        private const string ForbiddenCode = "Project.Forbidden";
        private const string MemberAlreadyExistsCode = "Project.MemberAlreadyExists";
        private const string CannotRemoveOwnerCode = "Project.CannotRemoveOwner";
        private const string MemberNotFoundCode = "Project.MemberNotFound";

        /// <summary>Returns a not-found error for the given project identifier.</summary>
        public static Error NotFoundError(ProjectId id) =>
            Error.NotFound(code: NotFoundCode, description: $"No Project with id {id} was found.");

        /// <summary>Returns a forbidden error when the caller lacks write access.</summary>
        public static Error ForbiddenError() =>
            Error.Forbidden(code: ForbiddenCode, description: "You do not have permission to modify this Project.");

        /// <summary>Returns an error when the user is already a member of the project.</summary>
        public static Error MemberAlreadyExistsError() =>
            Error.Conflict(code: MemberAlreadyExistsCode, description: "The user is already a member of this project.");

        /// <summary>Returns an error when trying to remove the project owner.</summary>
        public static Error CannotRemoveOwnerError() =>
            Error.Conflict(code: CannotRemoveOwnerCode, description: "Cannot remove the owner from a project.");

        /// <summary>Returns an error when the target member is not found in the project.</summary>
        public static Error MemberNotFoundError() =>
            Error.NotFound(code: MemberNotFoundCode, description: "The specified user is not a member of this project.");
    }
}
