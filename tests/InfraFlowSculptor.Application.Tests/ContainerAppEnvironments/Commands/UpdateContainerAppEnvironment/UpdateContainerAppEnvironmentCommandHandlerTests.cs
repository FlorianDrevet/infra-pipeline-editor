using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;

public sealed class UpdateContainerAppEnvironmentCommandHandlerTests
{
    private readonly IContainerAppEnvironmentRepository _containerAppEnvironmentRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly ILogAnalyticsWorkspaceRepository _logAnalyticsWorkspaceRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly LogAnalyticsWorkspace _logAnalyticsWorkspace;
    private readonly ContainerAppEnvironment _existingEntity;
    private readonly UpdateContainerAppEnvironmentCommand _command;
    private readonly UpdateContainerAppEnvironmentCommandHandler _sut;

    public UpdateContainerAppEnvironmentCommandHandlerTests()
    {
        _containerAppEnvironmentRepository = Substitute.For<IContainerAppEnvironmentRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _logAnalyticsWorkspaceRepository = Substitute.For<ILogAnalyticsWorkspaceRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _logAnalyticsWorkspace = LogAnalyticsWorkspace.Create(
            _resourceGroup.Id,
            new Name("law-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = ContainerAppEnvironment.Create(
            _resourceGroup.Id,
            new Name("cae-old"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateContainerAppEnvironmentCommand(
            _existingEntity.Id,
            new Name("cae-renamed"),
            new Location(Location.LocationEnum.WestEurope),
            _logAnalyticsWorkspace.Id.Value);
        _containerAppEnvironmentRepository.Update(Arg.Any<ContainerAppEnvironment>())
            .Returns(callInfo => (ContainerAppEnvironment)callInfo.Args()[0]);
        _sut = new UpdateContainerAppEnvironmentCommandHandler(
            _containerAppEnvironmentRepository, _resourceGroupRepository,
            _logAnalyticsWorkspaceRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppEnvironmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ContainerAppEnvironment?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _containerAppEnvironmentRepository.DidNotReceive().Update(Arg.Any<ContainerAppEnvironment>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppEnvironmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _containerAppEnvironmentRepository.DidNotReceive().Update(Arg.Any<ContainerAppEnvironment>());
    }

    [Fact]
    public async Task Given_LogAnalyticsWorkspaceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppEnvironmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _logAnalyticsWorkspaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((LogAnalyticsWorkspace?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _containerAppEnvironmentRepository.DidNotReceive().Update(Arg.Any<ContainerAppEnvironment>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _containerAppEnvironmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _logAnalyticsWorkspaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_logAnalyticsWorkspace);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _containerAppEnvironmentRepository.Received(1).Update(Arg.Is<ContainerAppEnvironment>(e =>
            e.Name.Value == "cae-renamed"));
        _mapper.Received(1).Map<ContainerAppEnvironmentResult>(Arg.Any<ContainerAppEnvironment>());
    }
}
