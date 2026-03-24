using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.FunctionApps.Requests;

/// <summary>Request body for creating a new Function App resource inside a Resource Group.</summary>
public class CreateFunctionAppRequest : FunctionAppRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Function App.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
