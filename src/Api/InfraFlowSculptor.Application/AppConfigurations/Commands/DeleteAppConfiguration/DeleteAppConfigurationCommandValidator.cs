using FluentValidation;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.DeleteAppConfiguration;

/// <summary>Validates the <see cref="DeleteAppConfigurationCommand"/>.</summary>
public sealed class DeleteAppConfigurationCommandValidator : AbstractValidator<DeleteAppConfigurationCommand>
{
    public DeleteAppConfigurationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
