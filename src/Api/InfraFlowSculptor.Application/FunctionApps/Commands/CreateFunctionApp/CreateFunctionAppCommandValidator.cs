using FluentValidation;
using InfraFlowSculptor.Domain.Common.Catalogs;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;

/// <summary>Validates the <see cref="CreateFunctionAppCommand"/> before it is handled.</summary>
public sealed class CreateFunctionAppCommandValidator : AbstractValidator<CreateFunctionAppCommand>
{
    /// <summary>Initializes validation rules for creating a Function App.</summary>
    public CreateFunctionAppCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotNull().WithMessage("Location is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotNull().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.AppServicePlanId)
            .NotEmpty().WithMessage("AppServicePlanId is required.");

        RuleFor(x => x.RuntimeStack)
            .NotEmpty().WithMessage("RuntimeStack is required.")
            .Must(v => Enum.TryParse<FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(v, out _))
            .WithMessage("RuntimeStack must be a valid value (DotNet, Node, Python, Java, PowerShell).");

        RuleFor(x => x.RuntimeVersion)
            .NotEmpty().WithMessage("RuntimeVersion is required.")
            .Must((cmd, version) =>
            {
                if (!Enum.TryParse<FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(cmd.RuntimeStack, out var stack))
                    return true; // RuntimeStack validation handles this
                return RuntimeVersionCatalog.IsValidFunctionAppVersion(stack, version);
            })
            .WithMessage("RuntimeVersion is not a valid version for the selected RuntimeStack.");
    }
}
