using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.ServiceBusNamespaceAggregate.Entities;

public sealed class ServiceBusTopicSubscriptionTests
{
    private const string TopicName = "topic-events";
    private const string SubscriptionName = "sub-billing";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var namespaceId = AzureResourceId.CreateUnique();

        // Act
        var sut = ServiceBusTopicSubscription.Create(namespaceId, TopicName, SubscriptionName);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.ServiceBusNamespaceId.Should().Be(namespaceId);
        sut.TopicName.Should().Be(TopicName);
        sut.SubscriptionName.Should().Be(SubscriptionName);
    }
}
