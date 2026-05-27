using FluentAssertions;
using InfraFlowSculptor.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace InfraFlowSculptor.Application.Tests.Common.Behaviors;

/// <summary>
/// Verifies that MediatR pipeline behaviors are registered in the correct order:
/// LoggingBehavior first, then ValidationBehavior, then PersonalAccessTokenScopeBehavior, then UnitOfWorkBehavior.
/// </summary>
public sealed class BehaviorRegistrationOrderTests
{
    [Fact]
    public void Given_ApplicationServices_When_Registered_Then_BehaviorsAreOrderedValidationThenPatScopeThenUnitOfWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddApplication();

        // Act
        var behaviorDescriptors = services
            .Where(sd => sd.ServiceType == typeof(IPipelineBehavior<,>))
            .ToList();

        // Assert
        behaviorDescriptors.Should().HaveCount(4, "exactly four pipeline behaviors should be registered");

        var firstBehavior = behaviorDescriptors[0].ImplementationType;
        var secondBehavior = behaviorDescriptors[1].ImplementationType;
        var thirdBehavior = behaviorDescriptors[2].ImplementationType;
        var fourthBehavior = behaviorDescriptors[3].ImplementationType;

        firstBehavior.Should().Be(typeof(LoggingBehavior<,>),
            "LoggingBehavior must wrap all other behaviors to capture full request timing");
        secondBehavior.Should().Be(typeof(ValidationBehavior<,>),
            "ValidationBehavior must run after logging to reject invalid commands before the UoW opens a transaction");
        thirdBehavior.Should().Be(typeof(PersonalAccessTokenScopeBehavior<,>),
            "PAT scope enforcement must run before handlers so unauthorized requests are rejected consistently");
        fourthBehavior.Should().Be(typeof(UnitOfWorkBehavior<,>),
            "UnitOfWorkBehavior must wrap the handler after validation has passed");
    }
}
