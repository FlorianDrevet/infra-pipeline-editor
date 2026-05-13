using FluentValidation;
using InfraFlowSculptor.Domain.Common.Catalogs;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.UpdateFunctionApp;

/// <summary>Validates the <see cref="UpdateFunctionAppCommand"/> before it is handled.</summary>
public sealed class UpdateFunctionAppCommandValidator : AbstractValidator<UpdateFunctionAppCommand>
{
    /// <summary>Initializes validation rules for updating a Function App.</summary>
    public UpdateFunctionAppCommandValidator()
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
            .Must(value => Enum.TryParse<FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(value, out _))
            .WithMessage("RuntimeStack must be a valid value (DotNet, Node, Python, Java, PowerShell).");

        RuleFor(x => x.RuntimeVersion)
            .NotEmpty().WithMessage("RuntimeVersion is required.")
            .Must((command, version) =>
            {
                if (!Enum.TryParse<FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(command.RuntimeStack, out var stack))
                {
                    return true;
                }

                return RuntimeVersionCatalog.IsValidFunctionAppVersion(stack, version);
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