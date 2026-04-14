using FluentValidation;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;

/// <summary>Validates the <see cref="AddRoleAssignmentCommand"/> before it is handled.</summary>
public sealed class AddRoleAssignmentCommandValidator : AbstractValidator<AddRoleAssignmentCommand>
{
    /// <summary>Initializes a new instance of the <see cref="AddRoleAssignmentCommandValidator"/> class.</summary>
    public AddRoleAssignmentCommandValidator()
    {
        RuleFor(x => x.RoleDefinitionId)
            .NotEmpty()
            .WithMessage("RoleDefinitionId is required.");

        RuleFor(x => x.ManagedIdentityType)
            .NotEmpty()
            .Must(value => Enum.TryParse<ManagedIdentityType.IdentityTypeEnum>(value, ignoreCase: true, out _))
            .WithMessage($"ManagedIdentityType must be one of: {string.Join(", ", Enum.GetNames<ManagedIdentityType.IdentityTypeEnum>())}.");

        RuleFor(x => x.TargetResourceId)
            .Must((command, targetResourceId) => targetResourceId != command.SourceResourceId)
            .WithMessage("Source and target resources must be different.");

        RuleFor(x => x.UserAssignedIdentityId)
            .NotNull()
            .WithMessage("UserAssignedIdentityId is required when ManagedIdentityType is UserAssigned.")
            .When(x => string.Equals(x.ManagedIdentityType, nameof(ManagedIdentityType.IdentityTypeEnum.UserAssigned), StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x)
            .Must(cmd => !(cmd.RoleDefinitionId == AzureRoleDefinitionCatalog.AcrPull
                           && string.Equals(cmd.ManagedIdentityType, nameof(ManagedIdentityType.IdentityTypeEnum.SystemAssigned), StringComparison.OrdinalIgnoreCase)))
            .WithMessage("The AcrPull role can only be assigned using a User-Assigned Identity.");
    }
}
