using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.ServiceBusNamespaceAggregate.Entities;

public sealed class ServiceBusQueueTests
{
    private const string QueueName = "queue-orders";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var namespaceId = AzureResourceId.CreateUnique();

        // Act
        var sut = ServiceBusQueue.Create(namespaceId, QueueName);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.ServiceBusNamespaceId.Should().Be(namespaceId);
        sut.Name.Should().Be(QueueName);
    }

    [Fact]
    public void Given_TwoCreateCalls_When_Compared_Then_ProducesDifferentIds()
    {
        // Arrange
        var namespaceId = AzureResourceId.CreateUnique();

        // Act
        var first = ServiceBusQueue.Create(namespaceId, QueueName);
        var second = ServiceBusQueue.Create(namespaceId, QueueName);

        // Assert
        first.Id.Should().NotBe(second.Id);
    }
}
