using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.CosmosDbs.Requests;

/// <summary>Request body for creating a new Cosmos DB account resource inside a Resource Group.</summary>
public class CreateCosmosDbRequest : CosmosDbRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Cosmos DB account.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
