using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;

/// <summary>
/// Validates the <see cref="CreateContainerRegistryCommand"/> before it is handled.
/// </summary>
public sealed class CreateContainerRegistryCommandValidator : CreateResourceCommandValidator<CreateContainerRegistryCommand>
{
}
