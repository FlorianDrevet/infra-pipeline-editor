using FluentValidation;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;

/// <summary>Validates the <see cref="UpdateRoleAssignmentIdentityCommand"/> before it is handled.</summary>
public sealed class UpdateRoleAssignmentIdentityCommandValidator : AbstractValidator<UpdateRoleAssignmentIdentityCommand>
{
    /// <summary>Initializes a new instance of the <see cref="UpdateRoleAssignmentIdentityCommandValidator"/> class.</summary>
    public UpdateRoleAssignmentIdentityCommandValidator()
    {
        RuleFor(x => x.ManagedIdentityType)
            .NotEmpty()
            .Must(value => Enum.TryParse<ManagedIdentityType.IdentityTypeEnum>(value, ignoreCase: true, out _))
            .WithMessage($"ManagedIdentityType must be one of: {string.Join(", ", Enum.GetNames<ManagedIdentityType.IdentityTypeEnum>())}.");

        RuleFor(x => x.UserAssignedIdentityId)
            .NotNull()
            .WithMessage("UserAssignedIdentityId is required when ManagedIdentityType is UserAssigned.")
            .When(x => string.Equals(x.ManagedIdentityType, nameof(ManagedIdentityType.IdentityTypeEnum.UserAssigned), StringComparison.OrdinalIgnoreCase));
    }
}
