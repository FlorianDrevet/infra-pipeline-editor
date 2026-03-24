using FluentValidation;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;

/// <summary>
/// Validates the <see cref="CreateSqlDatabaseCommand"/> before it is handled.
/// </summary>
public sealed class CreateSqlDatabaseCommandValidator
    : AbstractValidator<CreateSqlDatabaseCommand>
{
    public CreateSqlDatabaseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotNull().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.SqlServerId)
            .NotEmpty().WithMessage("SqlServerId is required.");

        RuleFor(x => x.Collation)
            .NotEmpty().WithMessage("Collation is required.")
            .MaximumLength(128).WithMessage("Collation must not exceed 128 characters.");
    }
}
