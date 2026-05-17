using FluentValidation;
using InfraFlowSculptor.Application.WebApps.Common;

namespace InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;

/// <summary>Validates the <see cref="CreateWebAppCommand"/> before it is handled.</summary>
public sealed class CreateWebAppCommandValidator : AbstractValidator<CreateWebAppCommand>
{
    /// <summary>Initializes validation rules for creating a Web App.</summary>
    public CreateWebAppCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        this.AddWebAppRules();
    }
}
