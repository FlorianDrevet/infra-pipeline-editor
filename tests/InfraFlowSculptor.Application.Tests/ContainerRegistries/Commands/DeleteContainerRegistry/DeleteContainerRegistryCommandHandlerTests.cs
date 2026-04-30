using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.DeleteContainerRegistry;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerRegistries.Commands.DeleteContainerRegistry;

public sealed class DeleteContainerRegistryCommandHandlerTests
{
    private readonly IContainerRegistryRepository _registryRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly ContainerRegistry _registry;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteContainerRegistryCommand _command;
    private readonly DeleteContainerRegistryCommandHandler _sut;

    public DeleteContainerRegistryCommandHandlerTests()
    {
        _registryRepository = Substitute.For<IContainerRegistryRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _registry = ContainerRegistry.Create(
            _resourceGroup.Id,
            new Name("acrshared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new DeleteContainerRegistryCommand(_registry.Id);
        _sut = new DeleteContainerRegistryCommandHandler(
            _registryRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_ContainerRegistryNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _registryRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ContainerRegistry?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _registryRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesContainerRegistryAsync()
    {
        // Arrange
        _registryRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_registry);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _registryRepository.Received(1).DeleteAsync(_registry.Id);
    }
}
