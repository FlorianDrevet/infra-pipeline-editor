using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.DeleteLogAnalyticsWorkspace;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using ApplicationInsightsEntity = InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ApplicationInsights;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.LogAnalyticsWorkspaces.Commands.DeleteLogAnalyticsWorkspace;

public sealed class DeleteLogAnalyticsWorkspaceCommandHandlerTests
{
    private readonly ILogAnalyticsWorkspaceRepository _logAnalyticsWorkspaceRepository;
    private readonly IApplicationInsightsRepository _applicationInsightsRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly LogAnalyticsWorkspace _logAnalyticsWorkspace;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteLogAnalyticsWorkspaceCommand _command;
    private readonly DeleteLogAnalyticsWorkspaceCommandHandler _sut;

    public DeleteLogAnalyticsWorkspaceCommandHandlerTests()
    {
        _logAnalyticsWorkspaceRepository = Substitute.For<ILogAnalyticsWorkspaceRepository>();
        _applicationInsightsRepository = Substitute.For<IApplicationInsightsRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _logAnalyticsWorkspace = LogAnalyticsWorkspace.Create(
            _resourceGroup.Id,
            new Name("law-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new DeleteLogAnalyticsWorkspaceCommand(_logAnalyticsWorkspace.Id);
        _applicationInsightsRepository.GetByLogAnalyticsWorkspaceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationInsightsEntity>());
        _sut = new DeleteLogAnalyticsWorkspaceCommandHandler(
            _logAnalyticsWorkspaceRepository,
            _applicationInsightsRepository,
            _resourceGroupRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_LogAnalyticsWorkspaceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _logAnalyticsWorkspaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((LogAnalyticsWorkspace?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _logAnalyticsWorkspaceRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _logAnalyticsWorkspaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_logAnalyticsWorkspace);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _logAnalyticsWorkspaceRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesLogAnalyticsWorkspaceAsync()
    {
        // Arrange
        _logAnalyticsWorkspaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_logAnalyticsWorkspace);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _logAnalyticsWorkspaceRepository.Received(1).DeleteAsync(_logAnalyticsWorkspace.Id);
    }

    [Fact]
    public async Task Given_DependentAppInsightsExist_When_Handle_Then_CascadeDeletesAppInsightsAsync()
    {
        // Arrange
        var appInsights1 = ApplicationInsightsEntity.Create(
            _resourceGroup.Id,
            new Name("ai-one"),
            new Location(Location.LocationEnum.FranceCentral),
            _logAnalyticsWorkspace.Id);
        var appInsights2 = ApplicationInsightsEntity.Create(
            _resourceGroup.Id,
            new Name("ai-two"),
            new Location(Location.LocationEnum.FranceCentral),
            _logAnalyticsWorkspace.Id);
        _logAnalyticsWorkspaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_logAnalyticsWorkspace);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _applicationInsightsRepository.GetByLogAnalyticsWorkspaceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationInsightsEntity> { appInsights1, appInsights2 });

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _applicationInsightsRepository.Received(1).DeleteAsync(appInsights1.Id);
        await _applicationInsightsRepository.Received(1).DeleteAsync(appInsights2.Id);
        await _logAnalyticsWorkspaceRepository.Received(1).DeleteAsync(_logAnalyticsWorkspace.Id);
    }
}
