using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ServiceBusNamespaceAggregate;

public sealed class ServiceBusNamespaceTests
{
    private const string DefaultName = "sb-prod";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const string DefaultQueueName = "queue-orders";
    private const string DefaultTopicName = "topic-events";
    private const string DefaultSubscriptionName = "sub-billing";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static ServiceBusNamespace CreateValidServiceBusNamespace(bool isExisting = false)
    {
        return ServiceBusNamespace.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();

        // Act
        var sut = ServiceBusNamespace.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue));

        // Assert
        sut.Id.Should().NotBeNull();
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
        sut.Queues.Should().BeEmpty();
        sut.TopicSubscriptions.Should().BeEmpty();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PopulatesCollection()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (string?)"Standard", (int?)null, (bool?)false, (bool?)false, (string?)"1.2"),
            (ProdEnvironment, (string?)"Premium", (int?)2, (bool?)true, (bool?)true, (string?)"1.2"),
        };

        // Act
        var sut = ServiceBusNamespace.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            environmentSettings: settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_IgnoresEnvironmentSettings()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (string?)"Standard", (int?)null, (bool?)false, (bool?)false, (string?)"1.2"),
        };

        // Act
        var sut = ServiceBusNamespace.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            environmentSettings: settings,
            isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsNameAndLocation()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();

        // Act
        sut.Update(new Name("sb-updated"), new Location(Location.LocationEnum.NorthEurope));

        // Assert
        sut.Name.Value.Should().Be("sb-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
    }

    // ─── SetEnvironmentSettings ────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "Premium", 2, true, true, "1.2");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntry()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();
        sut.SetEnvironmentSettings(ProdEnvironment, "Standard", null, false, false, "1.2");

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "Premium", 4, true, true, "1.2");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.Sku.Should().Be("Premium");
        entry.Capacity.Should().Be(4);
        entry.ZoneRedundant.Should().BeTrue();
    }

    [Fact]
    public void Given_IsExisting_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "Premium", 2, true, true, "1.2");

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── SetAllEnvironmentSettings ─────────────────────────────────────────

    [Fact]
    public void Given_NewSettings_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();
        sut.SetEnvironmentSettings(DevEnvironment, "Standard", null, false, false, "1.2");

        var newSettings = new[]
        {
            (ProdEnvironment, (string?)"Premium", (int?)2, (bool?)true, (bool?)true, (string?)"1.2"),
        };

        // Act
        sut.SetAllEnvironmentSettings(newSettings);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
    }

    // ─── AddQueue ──────────────────────────────────────────────────────────

    [Fact]
    public void Given_UniqueName_When_AddQueue_Then_ReturnsQueue()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();

        // Act
        var result = sut.AddQueue(DefaultQueueName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(DefaultQueueName);
        sut.Queues.Should().ContainSingle();
    }

    [Fact]
    public void Given_DuplicateName_When_AddQueue_Then_ReturnsError()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();
        sut.AddQueue(DefaultQueueName);

        // Act
        var result = sut.AddQueue(DefaultQueueName.ToUpperInvariant());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("ServiceBusNamespace.DuplicateQueueName");
        sut.Queues.Should().ContainSingle();
    }

    // ─── RemoveQueue ───────────────────────────────────────────────────────

    [Fact]
    public void Given_ExistingQueue_When_RemoveQueue_Then_RemovesIt()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();
        var added = sut.AddQueue(DefaultQueueName).Value;

        // Act
        var result = sut.RemoveQueue(added.Id);

        // Assert
        result.IsError.Should().BeFalse();
        sut.Queues.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownQueueId_When_RemoveQueue_Then_ReturnsError()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();

        // Act
        var result = sut.RemoveQueue(ServiceBusQueueId.CreateUnique());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("ServiceBusNamespace.QueueNotFound");
    }

    // ─── AddTopicSubscription ──────────────────────────────────────────────

    [Fact]
    public void Given_UniqueTopicSubscription_When_AddTopicSubscription_Then_ReturnsSubscription()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();

        // Act
        var result = sut.AddTopicSubscription(DefaultTopicName, DefaultSubscriptionName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TopicName.Should().Be(DefaultTopicName);
        result.Value.SubscriptionName.Should().Be(DefaultSubscriptionName);
        sut.TopicSubscriptions.Should().ContainSingle();
    }

    [Fact]
    public void Given_DuplicateTopicSubscription_When_AddTopicSubscription_Then_ReturnsError()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();
        sut.AddTopicSubscription(DefaultTopicName, DefaultSubscriptionName);

        // Act
        var result = sut.AddTopicSubscription(DefaultTopicName.ToUpperInvariant(), DefaultSubscriptionName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("ServiceBusNamespace.DuplicateTopicSubscription");
    }

    // ─── RemoveTopicSubscription ───────────────────────────────────────────

    [Fact]
    public void Given_ExistingTopicSubscription_When_RemoveTopicSubscription_Then_RemovesIt()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();
        var added = sut.AddTopicSubscription(DefaultTopicName, DefaultSubscriptionName).Value;

        // Act
        var result = sut.RemoveTopicSubscription(added.Id);

        // Assert
        result.IsError.Should().BeFalse();
        sut.TopicSubscriptions.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownSubscriptionId_When_RemoveTopicSubscription_Then_ReturnsError()
    {
        // Arrange
        var sut = CreateValidServiceBusNamespace();

        // Act
        var result = sut.RemoveTopicSubscription(ServiceBusTopicSubscriptionId.CreateUnique());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("ServiceBusNamespace.TopicSubscriptionNotFound");
    }
}
