using FluentValidation;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.RemovePrivateEndpointConfig;

/// <summary>Validates <see cref="RemovePrivateEndpointConfigCommand"/> inputs.</summary>
public sealed class RemovePrivateEndpointConfigCommandValidator : AbstractValidator<RemovePrivateEndpointConfigCommand>
{
    public RemovePrivateEndpointConfigCommandValidator()
    {
        RuleFor(x => x.InfraConfigId).NotNull();
        RuleFor(x => x.ResourceId).NotNull();
    }
}
