using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the Web App aggregate.</summary>
    public static class WebApp
    {
        private const string NotFoundCode = "WebApp.NotFound";

        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: NotFoundCode,
            description: $"A Web App with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
