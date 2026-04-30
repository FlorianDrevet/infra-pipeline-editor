using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.Errors;
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

namespace InfraFlowSculptor.Application.Tests.ContainerApps.Commands.CreateContainerApp;

public sealed class CreateContainerAppCommandHandlerTests
{
    private const string ContainerAppName = "ca-shared";

    private readonly IContainerAppRepository _containerAppRepository;
    private readonly IContainerAppEnvironmentRepository _environmentRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly ContainerAppEnvironment _environment;
    private readonly CreateContainerAppCommand _command;
    private readonly CreateContainerAppCommandHandler _sut;

    public CreateContainerAppCommandHandlerTests()
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
        _command = new CreateContainerAppCommand(
            _resourceGroup.Id,
            new Name(ContainerAppName),
            new Location(Location.LocationEnum.FranceCentral),
            ContainerAppEnvironmentId: _environment.Id.Value,
            ContainerRegistryId: null);
        _containerAppRepository.AddAsync(Arg.Any<ContainerApp>())
            .Returns(callInfo => Task.FromResult((ContainerApp)callInfo.Args()[0]));
        _sut = new CreateContainerAppCommandHandler(
            _containerAppRepository, _environmentRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _containerAppRepository.DidNotReceive().AddAsync(Arg.Any<ContainerApp>());
    }

    [Fact]
    public async Task Given_ContainerAppEnvironmentNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
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
        await _containerAppRepository.DidNotReceive().AddAsync(Arg.Any<ContainerApp>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsContainerAppAndMapsResultAsync()
    {
        // Arrange
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
        await _containerAppRepository.Received(1).AddAsync(Arg.Is<ContainerApp>(c =>
            c.ResourceGroupId == _resourceGroup.Id
            && c.Name.Value == ContainerAppName
            && c.ContainerAppEnvironmentId == _environment.Id));
        _mapper.Received(1).Map<ContainerAppResult>(Arg.Any<ContainerApp>());
    }
}
