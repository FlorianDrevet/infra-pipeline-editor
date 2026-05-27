using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;
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

namespace InfraFlowSculptor.Application.Tests.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;

public sealed class CreateLogAnalyticsWorkspaceCommandHandlerTests
{
    private const string WorkspaceName = "law-shared";

    private readonly ILogAnalyticsWorkspaceRepository _logAnalyticsWorkspaceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateLogAnalyticsWorkspaceCommand _command;
    private readonly CreateLogAnalyticsWorkspaceCommandHandler _sut;

    public CreateLogAnalyticsWorkspaceCommandHandlerTests()
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
        _command = new CreateLogAnalyticsWorkspaceCommand(
            _resourceGroup.Id,
            new Name(WorkspaceName),
            new Location(Location.LocationEnum.FranceCentral));
        _logAnalyticsWorkspaceRepository.Add(Arg.Any<LogAnalyticsWorkspace>())
            .Returns(callInfo => (LogAnalyticsWorkspace)callInfo.Args()[0]);
        _sut = new CreateLogAnalyticsWorkspaceCommandHandler(
            _logAnalyticsWorkspaceRepository, _resourceGroupRepository, _accessService, _mapper);
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
        _logAnalyticsWorkspaceRepository.DidNotReceive().Add(Arg.Any<LogAnalyticsWorkspace>());
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _logAnalyticsWorkspaceRepository.DidNotReceive().Add(Arg.Any<LogAnalyticsWorkspace>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsLogAnalyticsWorkspaceAndMapsResultAsync()
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
        _logAnalyticsWorkspaceRepository.Received(1).Add(Arg.Is<LogAnalyticsWorkspace>(law =>
            law.ResourceGroupId == _resourceGroup.Id && law.Name.Value == WorkspaceName));
        _mapper.Received(1).Map<LogAnalyticsWorkspaceResult>(Arg.Any<LogAnalyticsWorkspace>());
    }
}
