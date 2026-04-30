using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.EventHubNamespaceAggregate.Entities;

public sealed class EventHubConsumerGroupTests
{
    private const string EventHubName = "events";
    private const string ConsumerGroupName = "cg-billing";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var namespaceId = AzureResourceId.CreateUnique();

        // Act
        var sut = EventHubConsumerGroup.Create(namespaceId, EventHubName, ConsumerGroupName);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.EventHubNamespaceId.Should().Be(namespaceId);
        sut.EventHubName.Should().Be(EventHubName);
        sut.ConsumerGroupName.Should().Be(ConsumerGroupName);
    }
}
