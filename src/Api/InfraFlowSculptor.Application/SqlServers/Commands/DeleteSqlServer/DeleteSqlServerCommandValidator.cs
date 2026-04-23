using FluentValidation;

namespace InfraFlowSculptor.Application.SqlServers.Commands.DeleteSqlServer;

/// <summary>Validates the <see cref="DeleteSqlServerCommand"/>.</summary>
public sealed class DeleteSqlServerCommandValidator : AbstractValidator<DeleteSqlServerCommand>
{
    public DeleteSqlServerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
