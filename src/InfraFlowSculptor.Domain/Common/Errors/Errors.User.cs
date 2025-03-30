using ErrorOr;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class User
    {
        public static Error DuplicateEmailError() => Error.Conflict(
            code: "User.DuplicateEmail",
            description: "A user with the given email already exists."
        );

        public static Error NotFoundUserWithIdError() => Error.NotFound(
            code: "User.NotFoundUserWithId",
            description: "A user with the given id does not exist."
        );
    }
}