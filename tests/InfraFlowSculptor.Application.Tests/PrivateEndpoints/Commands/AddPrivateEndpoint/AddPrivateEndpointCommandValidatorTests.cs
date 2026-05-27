using FluentAssertions;
using InfraFlowSculptor.Application.PrivateEndpoints.Commands.AddPrivateEndpoint;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.PrivateEndpoints.Commands.AddPrivateEndpoint;

public sealed class AddPrivateEndpointCommandValidatorTests
{
    private readonly AddPrivateEndpointCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AddPrivateEndpointCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            "sqlServer");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullResourceId_When_Validate_Then_FailsOnResourceId()
    {
        // Arrange
        var command = new AddPrivateEndpointCommand(
            null!,
            AzureResourceId.CreateUnique(),
            "sqlServer");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddPrivateEndpointCommand.ResourceId));
    }

    [Fact]
    public void Given_NullSubnetId_When_Validate_Then_FailsOnSubnetId()
    {
        // Arrange
        var command = new AddPrivateEndpointCommand(
            AzureResourceId.CreateUnique(),
            null!,
            "sqlServer");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddPrivateEndpointCommand.SubnetId));
    }

    [Fact]
    public void Given_EmptyGroupId_When_Validate_Then_FailsOnGroupId()
    {
        // Arrange
        var command = new AddPrivateEndpointCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddPrivateEndpointCommand.GroupId));
    }

    [Fact]
    public void Given_GroupIdTooLong_When_Validate_Then_FailsOnGroupId()
    {
        // Arrange
        var command = new AddPrivateEndpointCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            new string('a', 101));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddPrivateEndpointCommand.GroupId));
    }

    [Fact]
    public void Given_CustomNicNameTooLong_When_Validate_Then_FailsOnCustomNetworkInterfaceName()
    {
        // Arrange
        var command = new AddPrivateEndpointCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            "sqlServer",
            CustomNetworkInterfaceName: new string('a', 81));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddPrivateEndpointCommand.CustomNetworkInterfaceName));
    }

    [Fact]
    public void Given_CustomNicNameAtMaxLength_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AddPrivateEndpointCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            "sqlServer",
            CustomNetworkInterfaceName: new string('a', 80));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
