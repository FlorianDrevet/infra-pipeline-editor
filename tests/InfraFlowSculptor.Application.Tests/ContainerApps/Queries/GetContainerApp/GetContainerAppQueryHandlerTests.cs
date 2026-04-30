using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Application.ContainerApps.Queries.GetContainerApp;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerApps.Queries.GetContainerApp;

public sealed class GetContainerAppQueryHandlerTests
{
    private readonly IContainerAppRepository _containerAppRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly ContainerApp _containerApp;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly GetContainerAppQuery _query;
    private readonly GetContainerAppQueryHandler _sut;

    public GetContainerAppQueryHandlerTests()
    {
        _containerAppRepository = Substitute.For<IContainerAppRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _containerApp = ContainerApp.Create(
            _resourceGroup.Id,
            new Name("ca-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            containerRegistryId: null,
            acrAuthMode: null);
        _query = new GetContainerAppQuery(_containerApp.Id);
        _sut = new GetContainerAppQueryHandler(
            _containerAppRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ContainerAppNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ContainerApp?)null);

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
        _containerAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_containerApp);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<ContainerAppResult>(_containerApp);
    }
}
