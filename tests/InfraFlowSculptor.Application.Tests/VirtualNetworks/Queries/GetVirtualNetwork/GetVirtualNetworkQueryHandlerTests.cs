using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Application.VirtualNetworks.Queries;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Queries.GetVirtualNetwork;

public sealed class GetVirtualNetworkQueryHandlerTests
{
    private readonly IVirtualNetworkRepository _vnetRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly VirtualNetwork _vnet;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly GetVirtualNetworkQuery _query;
    private readonly GetVirtualNetworkQueryHandler _sut;

    public GetVirtualNetworkQueryHandlerTests()
    {
        _vnetRepository = Substitute.For<IVirtualNetworkRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _vnet = VirtualNetwork.Create(
            _resourceGroup.Id,
            new Name("vnet-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _query = new GetVirtualNetworkQuery(_vnet.Id);
        _sut = new GetVirtualNetworkQueryHandler(
            _vnetRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_VnetNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _vnetRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((VirtualNetwork?)null);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _vnetRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _vnetRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.NotFound());

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
        _vnetRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<VirtualNetworkResult>(_vnet);
    }
}
