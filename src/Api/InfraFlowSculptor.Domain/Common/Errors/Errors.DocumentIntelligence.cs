using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the Document Intelligence aggregate.</summary>
    public static class DocumentIntelligence
    {
        private const string InvalidSkuCode = "DocumentIntelligence.InvalidSku";
        private const string InvalidPublicNetworkAccessCode = "DocumentIntelligence.InvalidPublicNetworkAccess";

        /// <summary>Returned when a Document Intelligence resource with the specified identifier does not exist.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "DocumentIntelligence.NotFound",
            description: $"A Document Intelligence resource with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        /// <summary>Returns an error when a SKU string cannot be parsed into a valid enum value.</summary>
        public static Error InvalidSku(string raw) =>
            Error.Validation(code: InvalidSkuCode, description: $"The SKU '{raw}' is not valid for Document Intelligence.");

        /// <summary>Returns an error when a public network access mode string cannot be parsed into a valid enum value.</summary>
        public static Error InvalidPublicNetworkAccess(string raw) =>
            Error.Validation(
                code: InvalidPublicNetworkAccessCode,
                description: $"The public network access mode '{raw}' is not valid.");
    }
}
