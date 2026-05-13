using FluentValidation;

namespace InfraFlowSculptor.Application.SqlServers.Commands.UpdateSqlServer;

/// <summary>Validates the <see cref="UpdateSqlServerCommand"/> before it is handled.</summary>
public sealed class UpdateSqlServerCommandValidator : AbstractValidator<UpdateSqlServerCommand>
{
    /// <summary>Initializes validation rules for updating a SQL Server.</summary>
    public UpdateSqlServerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version is required.");

        RuleFor(x => x.AdministratorLogin)
            .NotEmpty().WithMessage("AdministratorLogin is required.")
            .MaximumLength(128).WithMessage("AdministratorLogin must not exceed 128 characters.");
    }
}