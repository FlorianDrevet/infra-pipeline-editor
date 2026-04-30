using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.StorageAccountAggregate.Entities;

public sealed class BlobLifecycleRuleTests
{
    private const string RuleName = "delete-old-uploads";
    private const string ContainerNameA = "uploads";
    private const string ContainerNameB = "logs";
    private const int DefaultTtlDays = 30;

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var storageAccountId = AzureResourceId.CreateUnique();
        var containers = new[] { ContainerNameA, ContainerNameB };

        // Act
        var sut = BlobLifecycleRule.Create(storageAccountId, RuleName, containers, DefaultTtlDays);

        // Assert
        sut.StorageAccountId.Should().Be(storageAccountId);
        sut.RuleName.Should().Be(RuleName);
        sut.ContainerNames.Should().BeEquivalentTo(containers);
        sut.TimeToLiveInDays.Should().Be(DefaultTtlDays);
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_ReplacesAllProperties()
    {
        // Arrange
        var sut = BlobLifecycleRule.Create(
            AzureResourceId.CreateUnique(),
            RuleName,
            new[] { ContainerNameA },
            DefaultTtlDays);
        const string updatedRuleName = "delete-very-old";
        const int updatedTtl = 90;
        var updatedContainers = new[] { ContainerNameA, ContainerNameB };

        // Act
        sut.Update(updatedRuleName, updatedContainers, updatedTtl);

        // Assert
        sut.RuleName.Should().Be(updatedRuleName);
        sut.TimeToLiveInDays.Should().Be(updatedTtl);
        sut.ContainerNames.Should().BeEquivalentTo(updatedContainers);
    }

    [Fact]
    public void Given_InputContainerListMutated_When_Create_Then_StoresIndependentCopy()
    {
        // Arrange
        var storageAccountId = AzureResourceId.CreateUnique();
        var containers = new List<string> { ContainerNameA };
        var sut = BlobLifecycleRule.Create(storageAccountId, RuleName, containers, DefaultTtlDays);

        // Act
        containers.Add(ContainerNameB);

        // Assert
        sut.ContainerNames.Should().HaveCount(1);
    }
}
