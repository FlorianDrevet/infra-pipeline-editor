using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.ValueObjects;

public sealed class AppPipelineModeTests
{
    [Fact]
    public void Given_EnumDefinition_When_InspectingMembers_Then_ContainsIsolatedAndCombined()
    {
        // Act
        var values = Enum.GetValues<AppPipelineMode>();

        // Assert
        values.Should().BeEquivalentTo(new[] { AppPipelineMode.Isolated, AppPipelineMode.Combined });
    }
}
