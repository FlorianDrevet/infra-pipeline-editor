using FluentValidation;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.DeleteSqlDatabase;

/// <summary>Validates the <see cref="DeleteSqlDatabaseCommand"/>.</summary>
public sealed class DeleteSqlDatabaseCommandValidator : AbstractValidator<DeleteSqlDatabaseCommand>
{
    public DeleteSqlDatabaseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
