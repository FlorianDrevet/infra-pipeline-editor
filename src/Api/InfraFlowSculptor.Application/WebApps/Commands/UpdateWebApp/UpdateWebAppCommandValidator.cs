using FluentValidation;
using InfraFlowSculptor.Domain.Common.Catalogs;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.WebApps.Commands.UpdateWebApp;

/// <summary>Validates the <see cref="UpdateWebAppCommand"/> before it is handled.</summary>
public sealed class UpdateWebAppCommandValidator : AbstractValidator<UpdateWebAppCommand>
{
    /// <summary>Initializes validation rules for updating a Web App.</summary>
    public UpdateWebAppCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

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
