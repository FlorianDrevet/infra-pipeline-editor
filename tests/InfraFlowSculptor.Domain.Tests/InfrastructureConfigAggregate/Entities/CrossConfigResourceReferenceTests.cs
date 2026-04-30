using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.Entities;

public sealed class CrossConfigResourceReferenceTests
{
    [Fact]
    public void Given_ValidArguments_When_Create_Then_PopulatesAllFields()
    {
        // Arrange
        var infraConfigId = InfrastructureConfigId.CreateUnique();
        var targetConfigId = InfrastructureConfigId.CreateUnique();
        var targetResourceId = AzureResourceId.CreateUnique();

        // Act
        var sut = CrossConfigResourceReference.Create(infraConfigId, targetConfigId, targetResourceId);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.InfraConfigId.Should().Be(infraConfigId);
        sut.TargetConfigId.Should().Be(targetConfigId);
        sut.TargetResourceId.Should().Be(targetResourceId);
    }

    [Fact]
    public void Given_TwoCallsToCreate_When_Compared_Then_GenerateDistinctIdentifiers()
    {
        // Arrange
        var infraConfigId = InfrastructureConfigId.CreateUnique();
        var targetConfigId = InfrastructureConfigId.CreateUnique();
        var targetResourceId = AzureResourceId.CreateUnique();

        // Act
        var first = CrossConfigResourceReference.Create(infraConfigId, targetConfigId, targetResourceId);
        var second = CrossConfigResourceReference.Create(infraConfigId, targetConfigId, targetResourceId);

        // Assert
        first.Id.Equals(second.Id).Should().BeFalse();
    }
}
