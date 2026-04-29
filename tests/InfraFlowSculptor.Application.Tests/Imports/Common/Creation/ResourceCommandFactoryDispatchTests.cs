using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Common.Creation;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Imports.Common.Creation;

public sealed class ResourceCommandFactoryDispatchTests
{
    private readonly IMediator _mediator;

    public ResourceCommandFactoryDispatchTests()
    {
        _mediator = Substitute.For<IMediator>();
    }

    [Fact]
    public async Task Given_SupportedCreateCommand_When_ExecuteCommandAsync_Then_ReturnsCreatedResourceIdAsync()
    {
        // Arrange
        var resourceGroupId = new ResourceGroupId(Guid.NewGuid());
        var resourceId = new AzureResourceId(Guid.NewGuid());
        var command = new CreateKeyVaultCommand(
            resourceGroupId,
            new Name("test-kv"),
            new Location(Location.LocationEnum.WestEurope));

        var result = new KeyVaultResult(
            resourceId,
            resourceGroupId,
            new Name("test-kv"),
            new Location(Location.LocationEnum.WestEurope),
            false,
            false,
            false,
            false,
            false,
            false,
            []);

        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<KeyVaultResult>>(result));

        // Act
        var dispatchResult = await ResourceCommandFactory.ExecuteCommandAsync(
            _mediator,
            command,
            CancellationToken.None);

        // Assert
        dispatchResult.IsError.Should().BeFalse();
        dispatchResult.Value.Should().Be(resourceId.Value);
    }

    [Fact]
    public async Task Given_CommandErrors_When_ExecuteCommandAsync_Then_ReturnsErrorsAsync()
    {
        // Arrange
        var command = new CreateKeyVaultCommand(
            new ResourceGroupId(Guid.NewGuid()),
            new Name("test-kv"),
            new Location(Location.LocationEnum.WestEurope));

        var error = Error.Failure("KeyVault.CreateFailed", "Key Vault creation failed.");

        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<KeyVaultResult>>(error));

        // Act
        var dispatchResult = await ResourceCommandFactory.ExecuteCommandAsync(
            _mediator,
            command,
            CancellationToken.None);

        // Assert
        dispatchResult.IsError.Should().BeTrue();
        dispatchResult.Errors.Should().ContainSingle(e => e.Description == "Key Vault creation failed.");
    }
}