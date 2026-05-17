using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Marker interface for create-resource commands that share the common
/// <see cref="ResourceGroupId"/>, <see cref="Name"/>, and <see cref="Location"/> properties.
/// Used by <see cref="Validation.CreateResourceCommandValidator{T}"/> to apply shared rules.
/// </summary>
public interface ICreateResourceCommand
{
    /// <summary>The target resource group identifier.</summary>
    ResourceGroupId ResourceGroupId { get; }

    /// <summary>The resource name.</summary>
    Name Name { get; }

    /// <summary>The Azure region for the resource.</summary>
    Location Location { get; }
}
