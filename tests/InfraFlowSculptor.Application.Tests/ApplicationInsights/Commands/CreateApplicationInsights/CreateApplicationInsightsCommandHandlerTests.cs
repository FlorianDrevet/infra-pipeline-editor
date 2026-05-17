using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using ApplicationInsightsEntity = InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ApplicationInsights;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ApplicationInsights.Commands.CreateApplicationInsights;

public sealed class CreateApplicationInsightsCommandHandlerTests
{
    private const string AppInsightsName = "appi-shared";

    private readonly IApplicationInsightsRepository _applicationInsightsRepository;
    private readonly ILogAnalyticsWorkspaceRepository _logAnalyticsWorkspaceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly LogAnalyticsWorkspace _logAnalyticsWorkspace;
    private readonly CreateApplicationInsightsCommand _command;
    private readonly CreateApplicationInsightsCommandHandler _sut;

    public CreateApplicationInsightsCommandHandlerTests()
    {
        _applicationInsightsRepository = Substitute.For<IApplicationInsightsRepository>();
        _logAnalyticsWorkspaceRepository = Substitute.For<ILogAnalyticsWorkspaceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
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
        _command = new CreateApplicationInsightsCommand(
            _resourceGroup.Id,
            new Name(AppInsightsName),
            new Location(Location.LocationEnum.FranceCentral),
            Guid.NewGuid());
        _applicationInsightsRepository.Add(Arg.Any<ApplicationInsightsEntity>())
            .Returns(callInfo => (ApplicationInsightsEntity)callInfo.Args()[0]);
        _sut = new CreateApplicationInsightsCommandHandler(
            _applicationInsightsRepository, _logAnalyticsWorkspaceRepository,
            _resourceGroupRepository, _accessService, _mapper);
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
        _applicationInsightsRepository.DidNotReceive().Add(Arg.Any<ApplicationInsightsEntity>());
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
        _applicationInsightsRepository.DidNotReceive().Add(Arg.Any<ApplicationInsightsEntity>());
    }

    [Fact]
    public async Task Given_LogAnalyticsWorkspaceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
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
        _applicationInsightsRepository.DidNotReceive().Add(Arg.Any<ApplicationInsightsEntity>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsApplicationInsightsAndMapsResultAsync()
    {
        // Arrange
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
        _applicationInsightsRepository.Received(1).Add(Arg.Is<ApplicationInsightsEntity>(ai =>
            ai.ResourceGroupId == _resourceGroup.Id && ai.Name.Value == AppInsightsName));
        _mapper.Received(1).Map<ApplicationInsightsResult>(Arg.Any<ApplicationInsightsEntity>());
    }
}
