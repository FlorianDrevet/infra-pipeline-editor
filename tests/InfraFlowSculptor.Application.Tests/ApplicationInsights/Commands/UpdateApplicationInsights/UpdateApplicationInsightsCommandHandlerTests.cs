using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.ApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.UpdateApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainApplicationInsights = InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ApplicationInsights;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ApplicationInsights.Commands.UpdateApplicationInsights;

public sealed class UpdateApplicationInsightsCommandHandlerTests
{
    private readonly IApplicationInsightsRepository _applicationInsightsRepository;
    private readonly ILogAnalyticsWorkspaceRepository _logAnalyticsWorkspaceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly LogAnalyticsWorkspace _logAnalyticsWorkspace;
    private readonly DomainApplicationInsights _existingEntity;
    private readonly UpdateApplicationInsightsCommand _command;
    private readonly UpdateApplicationInsightsCommandHandler _sut;

    public UpdateApplicationInsightsCommandHandlerTests()
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
        _existingEntity = DomainApplicationInsights.Create(
            _resourceGroup.Id,
            new Name("ai-old"),
            new Location(Location.LocationEnum.FranceCentral),
            _logAnalyticsWorkspace.Id);
        _command = new UpdateApplicationInsightsCommand(
            _existingEntity.Id,
            new Name("ai-renamed"),
            new Location(Location.LocationEnum.WestEurope),
            _logAnalyticsWorkspace.Id.Value);
        _applicationInsightsRepository.Update(Arg.Any<DomainApplicationInsights>())
            .Returns(callInfo => (DomainApplicationInsights)callInfo.Args()[0]);
        _sut = new UpdateApplicationInsightsCommandHandler(
            _applicationInsightsRepository, _logAnalyticsWorkspaceRepository,
            _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _applicationInsightsRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainApplicationInsights?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _applicationInsightsRepository.DidNotReceive().Update(Arg.Any<DomainApplicationInsights>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _applicationInsightsRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _applicationInsightsRepository.DidNotReceive().Update(Arg.Any<DomainApplicationInsights>());
    }

    [Fact]
    public async Task Given_LogAnalyticsWorkspaceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _applicationInsightsRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
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
        _applicationInsightsRepository.DidNotReceive().Update(Arg.Any<DomainApplicationInsights>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _applicationInsightsRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
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
        _applicationInsightsRepository.Received(1).Update(Arg.Is<DomainApplicationInsights>(e =>
            e.Name.Value == "ai-renamed"));
        _mapper.Received(1).Map<ApplicationInsightsResult>(Arg.Any<DomainApplicationInsights>());
    }
}
