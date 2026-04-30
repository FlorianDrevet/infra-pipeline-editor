using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;

namespace InfraFlowSculptor.Domain.Tests.UserAssignedIdentityAggregate;

public sealed class UserAssignedIdentityTests
{
    private const string DefaultName = "uai-prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static UserAssignedIdentity CreateValid(bool isExisting = false)
    {
        return UserAssignedIdentity.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            isExisting: isExisting);
    }

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();

        // Act
        var sut = UserAssignedIdentity.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue));

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.IsExisting.Should().BeFalse();
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_FlagSet()
    {
        // Act
        var sut = CreateValid(isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsNameAndLocation()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        sut.Update(new Name("uai-updated"), new Location(Location.LocationEnum.NorthEurope));

        // Assert
        sut.Name.Value.Should().Be("uai-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
    }
}
