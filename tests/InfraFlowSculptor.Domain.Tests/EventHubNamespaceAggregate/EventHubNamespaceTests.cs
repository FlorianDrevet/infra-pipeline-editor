using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.EventHubNamespaceAggregate;

public sealed class EventHubNamespaceTests
{
    private const string DefaultName = "evhns-prod";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const string DefaultEventHubName = "events";
    private const string DefaultConsumerGroup = "cg-billing";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static EventHubNamespace CreateValid(bool isExisting = false)
    {
        return EventHubNamespace.Create(
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
        var sut = EventHubNamespace.Create(
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
        sut.EventHubs.Should().BeEmpty();
        sut.ConsumerGroups.Should().BeEmpty();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PopulatesCollection()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (string?)"Standard", (int?)1, (bool?)false, (bool?)false, (string?)"1.2", (bool?)false, (int?)null),
            (ProdEnvironment, (string?)"Premium", (int?)4, (bool?)true, (bool?)true, (string?)"1.2", (bool?)true, (int?)10),
        };

        // Act
        var sut = EventHubNamespace.Create(
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
            (DevEnvironment, (string?)"Standard", (int?)1, (bool?)false, (bool?)false, (string?)"1.2", (bool?)false, (int?)null),
        };

        // Act
        var sut = EventHubNamespace.Create(
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
        var sut = CreateValid();

        // Act
        sut.Update(new Name("evhns-updated"), new Location(Location.LocationEnum.NorthEurope));

        // Assert
        sut.Name.Value.Should().Be("evhns-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
    }

    // ─── SetEnvironmentSettings ────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "Premium", 4, true, true, "1.2", true, 10);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntry()
    {
        // Arrange
        var sut = CreateValid();
        sut.SetEnvironmentSettings(ProdEnvironment, "Standard", 1, false, false, "1.2", false, null);

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "Premium", 4, true, true, "1.2", true, 10);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().Sku.Should().Be("Premium");
        sut.EnvironmentSettings.Single().AutoInflateEnabled.Should().BeTrue();
    }

    [Fact]
    public void Given_IsExisting_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValid(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "Premium", 4, true, true, "1.2", true, 10);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── AddEventHub ───────────────────────────────────────────────────────

    [Fact]
    public void Given_UniqueName_When_AddEventHub_Then_ReturnsEventHub()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        var result = sut.AddEventHub(DefaultEventHubName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(DefaultEventHubName);
        sut.EventHubs.Should().ContainSingle();
    }

    [Fact]
    public void Given_DuplicateName_When_AddEventHub_Then_ReturnsError()
    {
        // Arrange
        var sut = CreateValid();
        sut.AddEventHub(DefaultEventHubName);

        // Act
        var result = sut.AddEventHub(DefaultEventHubName.ToUpperInvariant());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("EventHubNamespace.DuplicateEventHubName");
    }

    // ─── RemoveEventHub ────────────────────────────────────────────────────

    [Fact]
    public void Given_ExistingEventHub_When_RemoveEventHub_Then_RemovesIt()
    {
        // Arrange
        var sut = CreateValid();
        var added = sut.AddEventHub(DefaultEventHubName).Value;

        // Act
        var result = sut.RemoveEventHub(added.Id);

        // Assert
        result.IsError.Should().BeFalse();
        sut.EventHubs.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownEventHubId_When_RemoveEventHub_Then_ReturnsError()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        var result = sut.RemoveEventHub(EventHubId.CreateUnique());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("EventHubNamespace.EventHubNotFound");
    }

    // ─── AddConsumerGroup ──────────────────────────────────────────────────

    [Fact]
    public void Given_UniquePair_When_AddConsumerGroup_Then_ReturnsConsumerGroup()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        var result = sut.AddConsumerGroup(DefaultEventHubName, DefaultConsumerGroup);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.EventHubName.Should().Be(DefaultEventHubName);
        result.Value.ConsumerGroupName.Should().Be(DefaultConsumerGroup);
    }

    [Fact]
    public void Given_DuplicatePair_When_AddConsumerGroup_Then_ReturnsError()
    {
        // Arrange
        var sut = CreateValid();
        sut.AddConsumerGroup(DefaultEventHubName, DefaultConsumerGroup);

        // Act
        var result = sut.AddConsumerGroup(DefaultEventHubName.ToUpperInvariant(), DefaultConsumerGroup);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("EventHubNamespace.DuplicateConsumerGroup");
    }

    // ─── RemoveConsumerGroup ───────────────────────────────────────────────

    [Fact]
    public void Given_ExistingConsumerGroup_When_RemoveConsumerGroup_Then_RemovesIt()
    {
        // Arrange
        var sut = CreateValid();
        var added = sut.AddConsumerGroup(DefaultEventHubName, DefaultConsumerGroup).Value;

        // Act
        var result = sut.RemoveConsumerGroup(added.Id);

        // Assert
        result.IsError.Should().BeFalse();
        sut.ConsumerGroups.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownConsumerGroupId_When_RemoveConsumerGroup_Then_ReturnsError()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        var result = sut.RemoveConsumerGroup(EventHubConsumerGroupId.CreateUnique());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("EventHubNamespace.ConsumerGroupNotFound");
    }
}
