using FluentValidation;
using InfraFlowSculptor.Domain.Common.Catalogs;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;

/// <summary>Validates the <see cref="CreateWebAppCommand"/> before it is handled.</summary>
public sealed class CreateWebAppCommandValidator : AbstractValidator<CreateWebAppCommand>
{
    /// <summary>Initializes validation rules for creating a Web App.</summary>
    public CreateWebAppCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

        RuleFor(x => x.AppServicePlanId)
            .NotEmpty().WithMessage("AppServicePlanId is required.");

        RuleFor(x => x.RuntimeStack)
            .NotEmpty().WithMessage("RuntimeStack is required.")
            .Must(value => Enum.TryParse<WebAppRuntimeStack.WebAppRuntimeStackEnum>(value, out _))
            .WithMessage("RuntimeStack must be a valid value (DotNet, Node, Python, Java, Php).");

        RuleFor(x => x.RuntimeVersion)
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

        RuleFor(x => x.DeploymentMode)
            .NotEmpty().WithMessage("DeploymentMode is required.")
            .Must(value => Enum.TryParse<DeploymentMode.DeploymentModeType>(value, out _))
            .WithMessage("DeploymentMode must be a valid value (Code or Container).");

        RuleFor(x => x.ContainerRegistryId!.Value)
            .NotEmpty().WithMessage("ContainerRegistryId must not be empty when provided.")
            .When(x => x.ContainerRegistryId.HasValue);
    }
}
