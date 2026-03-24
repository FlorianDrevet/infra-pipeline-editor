using FluentValidation;

namespace InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;

/// <summary>
/// Validates the <see cref="CreateSqlServerCommand"/> before it is handled.
/// </summary>
public sealed class CreateSqlServerCommandValidator
    : AbstractValidator<CreateSqlServerCommand>
{
    public CreateSqlServerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotNull().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version is required.");

        RuleFor(x => x.AdministratorLogin)
            .NotEmpty().WithMessage("AdministratorLogin is required.")
            .MaximumLength(128).WithMessage("AdministratorLogin must not exceed 128 characters.");
    }
}
