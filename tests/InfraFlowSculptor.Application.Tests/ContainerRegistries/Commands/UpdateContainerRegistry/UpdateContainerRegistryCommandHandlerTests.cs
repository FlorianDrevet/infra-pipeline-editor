using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.UpdateContainerRegistry;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerRegistries.Commands.UpdateContainerRegistry;

public sealed class UpdateContainerRegistryCommandHandlerTests
{
    private readonly IContainerRegistryRepository _registryRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly ContainerRegistry _existingRegistry;
    private readonly UpdateContainerRegistryCommand _command;
    private readonly UpdateContainerRegistryCommandHandler _sut;

    public UpdateContainerRegistryCommandHandlerTests()
    {
        _registryRepository = Substitute.For<IContainerRegistryRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingRegistry = ContainerRegistry.Create(
            _resourceGroup.Id,
            new Name("acrshared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateContainerRegistryCommand(
            _existingRegistry.Id,
            new Name("acrrenamed"),
            new Location(Location.LocationEnum.WestEurope));
        _registryRepository.UpdateAsync(Arg.Any<ContainerRegistry>())
            .Returns(callInfo => Task.FromResult((ContainerRegistry)callInfo.Args()[0]));
        _sut = new UpdateContainerRegistryCommandHandler(
            _registryRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_RegistryNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _registryRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ContainerRegistry?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _registryRepository.DidNotReceive().UpdateAsync(Arg.Any<ContainerRegistry>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _registryRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingRegistry);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _registryRepository.DidNotReceive().UpdateAsync(Arg.Any<ContainerRegistry>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedRegistryAndMapsResultAsync()
    {
        // Arrange
        _registryRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingRegistry);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _registryRepository.Received(1).UpdateAsync(Arg.Is<ContainerRegistry>(cr =>
            cr.Name.Value == "acrrenamed"));
        _mapper.Received(1).Map<ContainerRegistryResult>(Arg.Any<ContainerRegistry>());
    }
}
