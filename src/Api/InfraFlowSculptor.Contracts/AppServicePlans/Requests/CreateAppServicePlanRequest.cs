using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.AppServicePlans.Requests;

/// <summary>Request body for creating a new App Service Plan resource inside a Resource Group.</summary>
public class CreateAppServicePlanRequest : AppServicePlanRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this App Service Plan.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>

    public bool IsExisting { get; init; } = false;

}
