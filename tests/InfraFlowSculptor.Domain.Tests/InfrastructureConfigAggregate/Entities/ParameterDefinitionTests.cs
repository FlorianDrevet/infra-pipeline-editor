using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.Entities;

public sealed class ParameterDefinitionTests
{
    [Fact]
    public void Given_NoArgs_When_Create_Then_GeneratesUniqueIdentifier()
    {
        // Act
        var sut = ParameterDefinition.Create();

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Given_TwoCallsToCreate_When_Compared_Then_GenerateDistinctIdentifiers()
    {
        // Act
        var first = ParameterDefinition.Create();
        var second = ParameterDefinition.Create();

        // Assert
        first.Id.Equals(second.Id).Should().BeFalse();
    }
}
