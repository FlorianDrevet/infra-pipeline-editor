using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DeleteInfraConfig;

/// <summary>Validates the <see cref="DeleteInfrastructureConfigCommand"/>.</summary>
public sealed class DeleteInfrastructureConfigCommandValidator : AbstractValidator<DeleteInfrastructureConfigCommand>
{
    public DeleteInfrastructureConfigCommandValidator()
    {
        RuleFor(x => x.InfraConfigId).NotEmpty().WithMessage("InfraConfigId is required.");
    }
}
