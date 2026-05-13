using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Application.ApplicationInsights.Queries.GetApplicationInsights;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainApplicationInsights = InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ApplicationInsights;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ApplicationInsights.Queries;

public sealed class GetApplicationInsightsQueryHandlerTests
{
    private readonly IApplicationInsightsRepository _applicationInsightsRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainApplicationInsights _applicationInsights;
    private readonly GetApplicationInsightsQuery _query;
    private readonly GetApplicationInsightsQueryHandler _sut;

    public GetApplicationInsightsQueryHandlerTests()
    {
        _applicationInsightsRepository = Substitute.For<IApplicationInsightsRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-observability"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _applicationInsights = DomainApplicationInsights.Create(
            _resourceGroup.Id,
            new Name("appi-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique());
        _query = new GetApplicationInsightsQuery(_applicationInsights.Id);
        _sut = new GetApplicationInsightsQueryHandler(
            _applicationInsightsRepository,
            _resourceGroupRepository,
            _accessService,
            _mapper);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsNotFoundToHideExistenceAsync()
    {
        // Arrange
        _applicationInsightsRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_applicationInsights);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_applicationInsights.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _applicationInsightsRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_MapsResultUsingReadOnlyLookupsAsync()
    {
        // Arrange
        _applicationInsightsRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_applicationInsights);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_applicationInsights.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<ApplicationInsightsResult>(_applicationInsights);
        await _applicationInsightsRepository.Received(1)
            .GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_applicationInsights.ResourceGroupId, Arg.Any<CancellationToken>());
        await _applicationInsightsRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }
}