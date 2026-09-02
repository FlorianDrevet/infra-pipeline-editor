using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateVirtualNetwork;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Commands.UpdateVirtualNetwork;

public sealed class UpdateVirtualNetworkCommandHandlerTests
{
    private readonly IVirtualNetworkRepository _virtualNetworkRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly VirtualNetwork _existingEntity;
    private readonly UpdateVirtualNetworkCommand _command;
    private readonly UpdateVirtualNetworkCommandHandler _sut;

    public UpdateVirtualNetworkCommandHandlerTests()
    {
        _virtualNetworkRepository = Substitute.For<IVirtualNetworkRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = VirtualNetwork.Create(
            _resourceGroup.Id,
            new Name("vnet-old"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateVirtualNetworkCommand(
            _existingEntity.Id,
            new Name("vnet-renamed"),
            new Location(Location.LocationEnum.WestEurope),
            EnvironmentSettings:
            [
                new VirtualNetworkEnvironmentConfigData("prod", ["10.0.0.0/16"], null, EnableDdosProtection: true),
            ]);
        _virtualNetworkRepository.Update(Arg.Any<VirtualNetwork>())
            .Returns(callInfo => (VirtualNetwork)callInfo.Args()[0]);
        _sut = new UpdateVirtualNetworkCommandHandler(
            _virtualNetworkRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _virtualNetworkRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((VirtualNetwork?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _virtualNetworkRepository.DidNotReceive().Update(Arg.Any<VirtualNetwork>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _virtualNetworkRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _virtualNetworkRepository.DidNotReceive().Update(Arg.Any<VirtualNetwork>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _virtualNetworkRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _virtualNetworkRepository.Received(1).Update(Arg.Is<VirtualNetwork>(e =>
            e.Name.Value == "vnet-renamed"
            && e.EnvironmentSettings.Any(es => es.EnableDdosProtection)));
        _mapper.Received(1).Map<VirtualNetworkResult>(Arg.Any<VirtualNetwork>());
    }
}
