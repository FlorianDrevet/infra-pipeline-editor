using FluentValidation;

namespace InfraFlowSculptor.Application.WebApps.Commands.DeleteWebApp;

/// <summary>Validates the <see cref="DeleteWebAppCommand"/>.</summary>
public sealed class DeleteWebAppCommandValidator : AbstractValidator<DeleteWebAppCommand>
{
    public DeleteWebAppCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
