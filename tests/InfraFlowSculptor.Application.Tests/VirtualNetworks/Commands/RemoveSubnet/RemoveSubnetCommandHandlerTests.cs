using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.RemoveSubnet;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Commands.RemoveSubnet;

public sealed class RemoveSubnetCommandHandlerTests
{
    private const string AddressPrefix = "10.0.1.0/24";

    private readonly IVirtualNetworkRepository _virtualNetworkRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly VirtualNetwork _vnet;
    private readonly Guid _subnetId;
    private readonly RemoveSubnetCommandHandler _sut;

    public RemoveSubnetCommandHandlerTests()
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
        _vnet = VirtualNetwork.Create(
            _resourceGroup.Id,
            new Name("vnet-shared"),
            new Location(Location.LocationEnum.FranceCentral));

        var subnet = _vnet.AddSubnet(
            new Name("subnet-app"),
            AddressPrefix,
            null,
            null,
            new PrivateEndpointNetworkPolicy(PrivateEndpointNetworkPolicy.Policy.Disabled),
            null);
        _subnetId = subnet.Id.Value;

        _sut = new RemoveSubnetCommandHandler(
            _virtualNetworkRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_RemovesSubnetAndReturnsResultAsync()
    {
        // Arrange
        var command = new RemoveSubnetCommand(_vnet.Id, _subnetId);

        _virtualNetworkRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _vnet.Subnets.Should().BeEmpty();
        _virtualNetworkRepository.Received(1).Update(Arg.Is<VirtualNetwork>(v => v.Id == _vnet.Id));
        _mapper.Received(1).Map<VirtualNetworkResult>(Arg.Any<VirtualNetwork>());
    }

    [Fact]
    public async Task Given_VirtualNetworkNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var command = new RemoveSubnetCommand(AzureResourceId.CreateUnique(), _subnetId);

        _virtualNetworkRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((VirtualNetwork?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _resourceGroupRepository.DidNotReceive().GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        _virtualNetworkRepository.DidNotReceive().Update(Arg.Any<VirtualNetwork>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var command = new RemoveSubnetCommand(_vnet.Id, _subnetId);

        _virtualNetworkRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _accessService.DidNotReceive().VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>());
        _virtualNetworkRepository.DidNotReceive().Update(Arg.Any<VirtualNetwork>());
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_PropagatesAuthErrorAsync()
    {
        // Arrange
        var command = new RemoveSubnetCommand(_vnet.Id, _subnetId);

        _virtualNetworkRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Forbidden());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        _vnet.Subnets.Should().ContainSingle();
        _virtualNetworkRepository.DidNotReceive().Update(Arg.Any<VirtualNetwork>());
    }

    [Fact]
    public async Task Given_UnknownSubnetId_When_Handle_Then_ThrowsInvalidOperationExceptionAsync()
    {
        // Arrange
        var unknownSubnetId = Guid.NewGuid();
        var command = new RemoveSubnetCommand(_vnet.Id, unknownSubnetId);

        _virtualNetworkRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Subnet '{unknownSubnetId}' not found.");
    }
}
