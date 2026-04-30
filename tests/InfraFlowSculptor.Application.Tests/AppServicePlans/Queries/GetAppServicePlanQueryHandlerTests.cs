using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.AppServicePlans.Queries;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppServicePlans.Queries;

public sealed class GetAppServicePlanQueryHandlerTests
{
    private readonly IAppServicePlanRepository _appServicePlanRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly AppServicePlan _plan;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly GetAppServicePlanQuery _query;
    private readonly GetAppServicePlanQueryHandler _sut;

    public GetAppServicePlanQueryHandlerTests()
    {
        _appServicePlanRepository = Substitute.For<IAppServicePlanRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _plan = AppServicePlan.Create(
            _resourceGroup.Id,
            new Name("asp-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            new AppServicePlanOsType(AppServicePlanOsType.AppServicePlanOsTypeEnum.Linux));
        _query = new GetAppServicePlanQuery(_plan.Id);
        _sut = new GetAppServicePlanQueryHandler(
            _appServicePlanRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_AppServicePlanNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _appServicePlanRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((AppServicePlan?)null);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_MapsResultAsync()
    {
        // Arrange
        _appServicePlanRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_plan);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<AppServicePlanResult>(_plan);
    }
}
