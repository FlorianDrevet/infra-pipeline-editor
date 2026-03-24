using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="ApplicationInsightsAggregate.ApplicationInsights"/> aggregate.</summary>
    public static class ApplicationInsights
    {
        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "ApplicationInsights.NotFound",
            description: $"An Application Insights resource with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
