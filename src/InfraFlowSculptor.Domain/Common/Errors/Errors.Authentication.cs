using ErrorOr;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class Authentication
    {
        public static Error InvalidUsername() => Error.Validation(
            code: "Auth.InvalidUsername",
            description: "Invalid username."
        );

        public static Error InvalidPassword() => Error.Validation(
            code: "Auth.InvalidPassword",
            description: "Invalid password."
        );
    }
}