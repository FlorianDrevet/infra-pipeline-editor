using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.EventHubNamespaceAggregate.Entities;

public sealed class EventHubTests
{
    private const string EventHubName = "events";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var namespaceId = AzureResourceId.CreateUnique();

        // Act
        var sut = EventHub.Create(namespaceId, EventHubName);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.EventHubNamespaceId.Should().Be(namespaceId);
        sut.Name.Should().Be(EventHubName);
    }
}
