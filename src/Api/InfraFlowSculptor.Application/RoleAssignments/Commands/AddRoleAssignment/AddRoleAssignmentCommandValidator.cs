using FluentValidation;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;

public class AddRoleAssignmentCommandValidator : AbstractValidator<AddRoleAssignmentCommand>
{
    public AddRoleAssignmentCommandValidator()
    {
        RuleFor(x => x.RoleDefinitionId)
            .NotEmpty()
            .WithMessage("RoleDefinitionId is required.");

        RuleFor(x => x.ManagedIdentityType)
            .NotEmpty()
            .Must(value => Enum.TryParse<ManagedIdentityType.IdentityTypeEnum>(value, ignoreCase: true, out _))
            .WithMessage($"ManagedIdentityType must be one of: {string.Join(", ", Enum.GetNames<ManagedIdentityType.IdentityTypeEnum>())}.");

        RuleFor(x => x)
            .Must(x => x.SourceResourceId != x.TargetResourceId)
            .WithMessage("Source and target resources must be different.");
    }
}
