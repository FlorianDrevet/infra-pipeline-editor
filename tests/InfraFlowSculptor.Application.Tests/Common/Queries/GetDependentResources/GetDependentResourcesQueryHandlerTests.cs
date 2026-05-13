using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Queries.GetDependentResources;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using ApplicationInsightsEntity = InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ApplicationInsights;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Common.Queries.GetDependentResources;

public sealed class GetDependentResourcesQueryHandlerTests
{
    private readonly ILogAnalyticsWorkspaceRepository _logAnalyticsWorkspaceRepository;
    private readonly IAppServicePlanRepository _appServicePlanRepository;
    private readonly ISqlServerRepository _sqlServerRepository;
    private readonly IApplicationInsightsRepository _applicationInsightsRepository;
    private readonly IWebAppRepository _webAppRepository;
    private readonly IFunctionAppRepository _functionAppRepository;
    private readonly ISqlDatabaseRepository _sqlDatabaseRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly LogAnalyticsWorkspace _workspace;
    private readonly GetDependentResourcesQuery _query;
    private readonly GetDependentResourcesQueryHandler _sut;

    public GetDependentResourcesQueryHandlerTests()
    {
        _logAnalyticsWorkspaceRepository = Substitute.For<ILogAnalyticsWorkspaceRepository>();
        _appServicePlanRepository = Substitute.For<IAppServicePlanRepository>();
        _sqlServerRepository = Substitute.For<ISqlServerRepository>();
        _applicationInsightsRepository = Substitute.For<IApplicationInsightsRepository>();
        _webAppRepository = Substitute.For<IWebAppRepository>();
        _functionAppRepository = Substitute.For<IFunctionAppRepository>();
        _sqlDatabaseRepository = Substitute.For<ISqlDatabaseRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-observability"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _workspace = LogAnalyticsWorkspace.Create(
            _resourceGroup.Id,
            new Name("law-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _query = new GetDependentResourcesQuery(_workspace.Id);
        _sut = new GetDependentResourcesQueryHandler(
            _logAnalyticsWorkspaceRepository,
            _appServicePlanRepository,
            _sqlServerRepository,
            _applicationInsightsRepository,
            _webAppRepository,
            _functionAppRepository,
            _sqlDatabaseRepository,
            _resourceGroupRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_LogAnalyticsWorkspaceParent_When_Handle_Then_UsesReadOnlyParentAndResourceGroupLookupsAsync()
    {
        // Arrange
        _logAnalyticsWorkspaceRepository.GetByIdReadOnlyAsync(_workspace.Id, Arg.Any<CancellationToken>())
            .Returns(_workspace);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_workspace.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _applicationInsightsRepository.GetByLogAnalyticsWorkspaceIdAsync(_workspace.Id, Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationInsightsEntity>());

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        await _logAnalyticsWorkspaceRepository.Received(1)
            .GetByIdReadOnlyAsync(_workspace.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_workspace.ResourceGroupId, Arg.Any<CancellationToken>());
        await _logAnalyticsWorkspaceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }
}