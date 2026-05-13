using FluentValidation;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.UpdateSqlDatabase;

/// <summary>Validates the <see cref="UpdateSqlDatabaseCommand"/> before it is handled.</summary>
public sealed class UpdateSqlDatabaseCommandValidator : AbstractValidator<UpdateSqlDatabaseCommand>
{
    /// <summary>Initializes validation rules for updating a SQL Database.</summary>
    public UpdateSqlDatabaseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

        RuleFor(x => x.SqlServerId)
            .NotEmpty().WithMessage("SqlServerId is required.");

        RuleFor(x => x.Collation)
            .NotEmpty().WithMessage("Collation is required.");
    }
}