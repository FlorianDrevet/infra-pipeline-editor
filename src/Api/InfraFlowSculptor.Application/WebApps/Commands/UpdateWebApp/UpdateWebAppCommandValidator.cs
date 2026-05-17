using FluentValidation;
using InfraFlowSculptor.Application.WebApps.Common;

namespace InfraFlowSculptor.Application.WebApps.Commands.UpdateWebApp;

/// <summary>Validates the <see cref="UpdateWebAppCommand"/> before it is handled.</summary>
public sealed class UpdateWebAppCommandValidator : AbstractValidator<UpdateWebAppCommand>
{
    /// <summary>Initializes validation rules for updating a Web App.</summary>
    public UpdateWebAppCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        this.AddWebAppRules();
    }
}
