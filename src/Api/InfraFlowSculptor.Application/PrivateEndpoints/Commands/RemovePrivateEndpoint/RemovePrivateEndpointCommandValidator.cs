using FluentValidation;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Commands.RemovePrivateEndpoint;

/// <summary>Validates the RemovePrivateEndpointCommand.</summary>
public sealed class RemovePrivateEndpointCommandValidator : AbstractValidator<RemovePrivateEndpointCommand>
{
    public RemovePrivateEndpointCommandValidator()
    {
        RuleFor(x => x.ResourceId).NotEmpty().WithMessage("ResourceId is required.");
        RuleFor(x => x.ConfigId).NotEmpty().WithMessage("ConfigId is required.");
    }
}
