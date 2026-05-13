using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Queries.GetLogAnalyticsWorkspace;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.LogAnalyticsWorkspaces.Queries;

public sealed class GetLogAnalyticsWorkspaceQueryHandlerTests
{
    private readonly ILogAnalyticsWorkspaceRepository _workspaceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly LogAnalyticsWorkspace _workspace;
    private readonly GetLogAnalyticsWorkspaceQuery _query;
    private readonly GetLogAnalyticsWorkspaceQueryHandler _sut;

    public GetLogAnalyticsWorkspaceQueryHandlerTests()
    {
        _workspaceRepository = Substitute.For<ILogAnalyticsWorkspaceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-observability"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _workspace = LogAnalyticsWorkspace.Create(
            _resourceGroup.Id,
            new Name("law-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _query = new GetLogAnalyticsWorkspaceQuery(_workspace.Id);
        _sut = new GetLogAnalyticsWorkspaceQueryHandler(
            _workspaceRepository,
            _resourceGroupRepository,
            _accessService,
            _mapper);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsNotFoundToHideExistenceAsync()
    {
        // Arrange
        _workspaceRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_workspace);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_workspace.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _workspaceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_MapsResultUsingReadOnlyLookupsAsync()
    {
        // Arrange
        _workspaceRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_workspace);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_workspace.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<LogAnalyticsWorkspaceResult>(_workspace);
        await _workspaceRepository.Received(1)
            .GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_workspace.ResourceGroupId, Arg.Any<CancellationToken>());
        await _workspaceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }
}