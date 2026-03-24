using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the Function App aggregate.</summary>
    public static class FunctionApp
    {
        private const string NotFoundCode = "FunctionApp.NotFound";

        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: NotFoundCode,
            description: $"A Function App with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
