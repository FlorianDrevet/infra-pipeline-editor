using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the SQL Database aggregate.</summary>
    public static class SqlDatabase
    {
        private const string NotFoundCode = "SqlDatabase.NotFound";

        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: NotFoundCode,
            description: $"A SQL Database with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
