using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="LogAnalyticsWorkspaceAggregate.LogAnalyticsWorkspace"/> aggregate.</summary>
    public static class LogAnalyticsWorkspace
    {
        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "LogAnalyticsWorkspace.NotFound",
            description: $"A Log Analytics Workspace with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
