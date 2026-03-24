using ErrorOr;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for Git repository configuration and push operations.</summary>
    public static class GitRepository
    {
        private const string NotConfiguredCode = "GitRepository.NotConfigured";
        private const string InvalidRepositoryUrlCode = "GitRepository.InvalidRepositoryUrl";
        private const string PushFailedCode = "GitRepository.PushFailed";
        private const string SecretRetrievalFailedCode = "GitRepository.SecretRetrievalFailed";
        private const string ConnectionTestFailedCode = "GitRepository.ConnectionTestFailed";

        /// <summary>Returns an error when no Git config exists on the project.</summary>
        public static Error NotConfigured() =>
            Error.NotFound(code: NotConfiguredCode, description: "No Git repository configuration found for this project.");

        /// <summary>Returns a validation error for an invalid repository URL.</summary>
        public static Error InvalidRepositoryUrl() =>
            Error.Validation(code: InvalidRepositoryUrlCode, description: "The repository URL format is invalid.");

        /// <summary>Returns a failure error when pushing files to Git fails.</summary>
        public static Error PushFailed(string reason) =>
            Error.Failure(code: PushFailedCode, description: $"Failed to push files to Git repository: {reason}");

        /// <summary>Returns a failure error when the Key Vault secret cannot be retrieved.</summary>
        public static Error SecretRetrievalFailed() =>
            Error.Failure(code: SecretRetrievalFailedCode, description: "Failed to retrieve the authentication token from Key Vault.");

        /// <summary>Returns a failure error when the Git connection test fails.</summary>
        public static Error ConnectionTestFailed(string reason) =>
            Error.Failure(code: ConnectionTestFailedCode, description: $"Git repository connection test failed: {reason}");
    }
}
