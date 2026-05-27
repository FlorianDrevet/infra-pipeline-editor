using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;

public sealed class UpdateLogAnalyticsWorkspaceCommandHandlerTests
{
    private readonly ILogAnalyticsWorkspaceRepository _logAnalyticsWorkspaceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly LogAnalyticsWorkspace _existingEntity;
    private readonly UpdateLogAnalyticsWorkspaceCommand _command;
    private readonly UpdateLogAnalyticsWorkspaceCommandHandler _sut;

    public UpdateLogAnalyticsWorkspaceCommandHandlerTests()
    {
        _logAnalyticsWorkspaceRepository = Substitute.For<ILogAnalyticsWorkspaceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = LogAnalyticsWorkspace.Create(
            _resourceGroup.Id,
            new Name("law-old"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateLogAnalyticsWorkspaceCommand(
            _existingEntity.Id,
            new Name("law-renamed"),
            new Location(Location.LocationEnum.WestEurope));
        _logAnalyticsWorkspaceRepository.Update(Arg.Any<LogAnalyticsWorkspace>())
            .Returns(callInfo => (LogAnalyticsWorkspace)callInfo.Args()[0]);
        _sut = new UpdateLogAnalyticsWorkspaceCommandHandler(
            _logAnalyticsWorkspaceRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _logAnalyticsWorkspaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((LogAnalyticsWorkspace?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _logAnalyticsWorkspaceRepository.DidNotReceive().Update(Arg.Any<LogAnalyticsWorkspace>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _logAnalyticsWorkspaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _logAnalyticsWorkspaceRepository.DidNotReceive().Update(Arg.Any<LogAnalyticsWorkspace>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _logAnalyticsWorkspaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _logAnalyticsWorkspaceRepository.Received(1).Update(Arg.Is<LogAnalyticsWorkspace>(e =>
            e.Name.Value == "law-renamed"));
        _mapper.Received(1).Map<LogAnalyticsWorkspaceResult>(Arg.Any<LogAnalyticsWorkspace>());
    }
}
