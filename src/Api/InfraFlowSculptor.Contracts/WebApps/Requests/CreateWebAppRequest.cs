using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.WebApps.Requests;

/// <summary>Request body for creating a new Web App resource inside a Resource Group.</summary>
public class CreateWebAppRequest : WebAppRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Web App.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
