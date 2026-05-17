using FluentAssertions;
using InfraFlowSculptor.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace InfraFlowSculptor.Application.Tests.Common.Behaviors;

/// <summary>
/// Verifies that MediatR pipeline behaviors are registered in the correct order:
/// ValidationBehavior first, then UnitOfWorkBehavior.
/// </summary>
public sealed class BehaviorRegistrationOrderTests
{
    [Fact]
    public void Given_ApplicationServices_When_Registered_Then_ValidationBehaviorPrecedesUnitOfWorkBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddApplication();

        // Act
        var behaviorDescriptors = services
            .Where(sd => sd.ServiceType == typeof(IPipelineBehavior<,>))
            .ToList();

        // Assert
        behaviorDescriptors.Should().HaveCount(2, "exactly two pipeline behaviors should be registered");

        var firstBehavior = behaviorDescriptors[0].ImplementationType;
        var secondBehavior = behaviorDescriptors[1].ImplementationType;

        firstBehavior.Should().Be(typeof(ValidationBehavior<,>),
            "ValidationBehavior must run first to reject invalid commands before the UoW opens a transaction");
        secondBehavior.Should().Be(typeof(UnitOfWorkBehavior<,>),
            "UnitOfWorkBehavior must wrap the handler after validation has passed");
    }
}
