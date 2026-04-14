using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;

/// <summary>Validates the <see cref="GenerateProjectBicepCommand"/> before it is handled.</summary>
public sealed class GenerateProjectBicepCommandValidator
    : AbstractValidator<GenerateProjectBicepCommand>
{
    public GenerateProjectBicepCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}
