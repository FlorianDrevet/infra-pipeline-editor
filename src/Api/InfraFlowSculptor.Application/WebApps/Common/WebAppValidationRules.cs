using FluentValidation;
using InfraFlowSculptor.Domain.Common.Catalogs;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.WebApps.Common;

/// <summary>
/// Registers validation rules shared between <c>CreateWebAppCommandValidator</c>
/// and <c>UpdateWebAppCommandValidator</c>.
/// </summary>
public static class WebAppValidationRules
{
    /// <summary>
    /// Adds the common Web App validation rules (Name, Location, AppServicePlanId,
    /// RuntimeStack, RuntimeVersion, DeploymentMode, ContainerRegistryId) to the validator.
    /// </summary>
    /// <typeparam name="T">A command type implementing <see cref="IWebAppCommandProperties"/>.</typeparam>
    public static void AddWebAppRules<T>(this AbstractValidator<T> validator)
        where T : IWebAppCommandProperties
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        validator.RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

        validator.RuleFor(x => x.AppServicePlanId)
            .NotEmpty().WithMessage("AppServicePlanId is required.");

        validator.RuleFor(x => x.RuntimeStack)
            .NotEmpty().WithMessage("RuntimeStack is required.")
            .Must(value => Enum.TryParse<WebAppRuntimeStack.WebAppRuntimeStackEnum>(value, out _))
            .WithMessage("RuntimeStack must be a valid value (DotNet, Node, Python, Java, Php).");

        validator.RuleFor(x => x.RuntimeVersion)
            .NotEmpty().WithMessage("RuntimeVersion is required.")
            .Must((command, version) =>
            {
                if (!Enum.TryParse<WebAppRuntimeStack.WebAppRuntimeStackEnum>(command.RuntimeStack, out var stack))
                {
                    return true;
                }

                return RuntimeVersionCatalog.IsValidWebAppVersion(stack, version);
            })
            .WithMessage("RuntimeVersion is not a valid version for the selected RuntimeStack.");

        validator.RuleFor(x => x.DeploymentMode)
            .NotEmpty().WithMessage("DeploymentMode is required.")
            .Must(value => Enum.TryParse<DeploymentMode.DeploymentModeType>(value, out _))
            .WithMessage("DeploymentMode must be a valid value (Code or Container).");

        validator.RuleFor(x => x.ContainerRegistryId!.Value)
            .NotEmpty().WithMessage("ContainerRegistryId must not be empty when provided.")
            .When(x => x.ContainerRegistryId.HasValue);
    }
}
