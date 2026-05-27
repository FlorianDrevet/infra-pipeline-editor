using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerApps.Commands.UpdateContainerApp;

public sealed class UpdateContainerAppCommandHandlerTests
{
    private readonly IContainerAppRepository _containerAppRepository;
    private readonly IContainerAppEnvironmentRepository _environmentRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly ContainerAppEnvironment _environment;
    private readonly ContainerApp _existingEntity;
    private readonly UpdateContainerAppCommand _command;
    private readonly UpdateContainerAppCommandHandler _sut;

    public UpdateContainerAppCommandHandlerTests()
    {
        _containerAppRepository = Substitute.For<IContainerAppRepository>();
        _environmentRepository = Substitute.For<IContainerAppEnvironmentRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _environment = ContainerAppEnvironment.Create(
            _resourceGroup.Id,
            new Name("cae-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = ContainerApp.Create(
            _resourceGroup.Id,
            new Name("ca-old"),
            new Location(Location.LocationEnum.FranceCentral),
            _environment.Id,
            containerRegistryId: null,
            acrAuthMode: null);
        _command = new UpdateContainerAppCommand(
            _existingEntity.Id,
            new Name("ca-renamed"),
            new Location(Location.LocationEnum.WestEurope),
            ContainerAppEnvironmentId: _environment.Id.Value,
            ContainerRegistryId: null);
        _containerAppRepository.Update(Arg.Any<ContainerApp>())
            .Returns(callInfo => (ContainerApp)callInfo.Args()[0]);
        _sut = new UpdateContainerAppCommandHandler(
            _containerAppRepository, _environmentRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ContainerApp?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _containerAppRepository.DidNotReceive().Update(Arg.Any<ContainerApp>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _containerAppRepository.DidNotReceive().Update(Arg.Any<ContainerApp>());
    }

    [Fact]
    public async Task Given_ContainerAppEnvironmentNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _environmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ContainerAppEnvironment?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _containerAppRepository.DidNotReceive().Update(Arg.Any<ContainerApp>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _containerAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _environmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_environment);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _containerAppRepository.Received(1).Update(Arg.Is<ContainerApp>(c =>
            c.Name.Value == "ca-renamed"));
        _mapper.Received(1).Map<ContainerAppResult>(Arg.Any<ContainerApp>());
    }
}
