using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Common.Creation;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Imports.Common.Creation;

public sealed class ResourceCommandFactoryDispatchTests
{
    private readonly ISender _mediator;
    private readonly ResourceGroupId _resourceGroupId;
    private readonly Name _resourceName;
    private readonly Location _location;
    private readonly ResourceCreationContext _context;

    public ResourceCommandFactoryDispatchTests()
    {
        _mediator = Substitute.For<ISender>();
        _resourceGroupId = new ResourceGroupId(Guid.NewGuid());
        _resourceName = new Name("test-kv");
        _location = new Location(Location.LocationEnum.WestEurope);
        _context = new ResourceCreationContext(new Dictionary<string, Guid>());
    }

    [Fact]
    public async Task Given_SupportedResourceType_When_CreateResourceAsync_Then_ReturnsCreatedResourceIdAsync()
    {
        // Arrange
        var resourceId = new AzureResourceId(Guid.NewGuid());
        var result = new KeyVaultResult(
            resourceId,
            _resourceGroupId,
            _resourceName,
            _location,
            false,
            false,
            false,
            false,
            false,
            false,
            []);

        _mediator.Send(Arg.Any<CreateKeyVaultCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<KeyVaultResult>>(result));

        // Act
        var createTask = ResourceCommandFactory.CreateResourceAsync(
            _mediator,
            AzureResourceTypes.KeyVault,
            _resourceGroupId,
            _resourceName,
            _location,
            _context,
            CancellationToken.None);

        // Assert
        createTask.Should().NotBeNull();
        var dispatchResult = await createTask!;
        dispatchResult.IsError.Should().BeFalse();
        dispatchResult.Value.Should().Be(resourceId.Value);
    }

    [Fact]
    public async Task Given_CommandErrors_When_CreateResourceAsync_Then_ReturnsErrorsAsync()
    {
        // Arrange
        var error = Error.Failure("KeyVault.CreateFailed", "Key Vault creation failed.");

        _mediator.Send(Arg.Any<CreateKeyVaultCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<KeyVaultResult>>(error));

        // Act
        var createTask = ResourceCommandFactory.CreateResourceAsync(
            _mediator,
            AzureResourceTypes.KeyVault,
            _resourceGroupId,
            _resourceName,
            _location,
            _context,
            CancellationToken.None);

        // Assert
        createTask.Should().NotBeNull();
        var dispatchResult = await createTask!;
        dispatchResult.IsError.Should().BeTrue();
        dispatchResult.Errors.Should().ContainSingle(e => e.Description == "Key Vault creation failed.");
    }

    [Fact]
    public void Given_UnknownResourceType_When_CreateResourceAsync_Then_ReturnsNull()
    {
        // Act
        var createTask = ResourceCommandFactory.CreateResourceAsync(
            _mediator,
            "UnknownType",
            _resourceGroupId,
            _resourceName,
            _location,
            _context,
            CancellationToken.None);

        // Assert
        createTask.Should().BeNull();
    }
}