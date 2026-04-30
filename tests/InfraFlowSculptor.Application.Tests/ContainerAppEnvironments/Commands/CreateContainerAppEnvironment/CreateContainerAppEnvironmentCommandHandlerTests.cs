using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;

public sealed class CreateContainerAppEnvironmentCommandHandlerTests
{
    private const string EnvironmentName = "cae-shared";

    private readonly IContainerAppEnvironmentRepository _environmentRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly ILogAnalyticsWorkspaceRepository _logAnalyticsWorkspaceRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateContainerAppEnvironmentCommand _command;
    private readonly CreateContainerAppEnvironmentCommandHandler _sut;

    public CreateContainerAppEnvironmentCommandHandlerTests()
    {
        _environmentRepository = Substitute.For<IContainerAppEnvironmentRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _logAnalyticsWorkspaceRepository = Substitute.For<ILogAnalyticsWorkspaceRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _command = new CreateContainerAppEnvironmentCommand(
            _resourceGroup.Id,
            new Name(EnvironmentName),
            new Location(Location.LocationEnum.FranceCentral));
        _environmentRepository.AddAsync(Arg.Any<ContainerAppEnvironment>())
            .Returns(callInfo => Task.FromResult((ContainerAppEnvironment)callInfo.Args()[0]));
        _sut = new CreateContainerAppEnvironmentCommandHandler(
            _environmentRepository, _resourceGroupRepository, _logAnalyticsWorkspaceRepository, _accessService, _mapper);
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
        await _environmentRepository.DidNotReceive().AddAsync(Arg.Any<ContainerAppEnvironment>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsEnvironmentAndMapsResultAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _environmentRepository.Received(1).AddAsync(Arg.Is<ContainerAppEnvironment>(e =>
            e.ResourceGroupId == _resourceGroup.Id && e.Name.Value == EnvironmentName));
        _mapper.Received(1).Map<ContainerAppEnvironmentResult>(Arg.Any<ContainerAppEnvironment>());
    }
}
