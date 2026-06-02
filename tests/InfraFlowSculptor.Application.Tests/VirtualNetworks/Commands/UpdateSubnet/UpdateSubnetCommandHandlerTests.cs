using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateSubnet;
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

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Commands.UpdateSubnet;

public sealed class UpdateSubnetCommandHandlerTests
{
    private const string UpdatedSubnetName = "subnet-app-updated";
    private const string AddressPrefix = "10.0.1.0/24";
    private const string ValidPrivateEndpointPolicy = "Disabled";
    private const string ValidDelegation = "AppEnvironments";

    private readonly IVirtualNetworkRepository _virtualNetworkRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly VirtualNetwork _vnet;
    private readonly Guid _subnetId;
    private readonly UpdateSubnetCommandHandler _sut;

    public UpdateSubnetCommandHandlerTests()
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

        _sut = new UpdateSubnetCommandHandler(
            _virtualNetworkRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_UpdatesSubnetAndReturnsResultAsync()
    {
        // Arrange
        var command = new UpdateSubnetCommand(
            _vnet.Id,
            _subnetId,
            UpdatedSubnetName,
            AddressPrefix,
            null,
            null,
            ValidPrivateEndpointPolicy,
            null);

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
        _vnet.Subnets.Should().ContainSingle(s => s.Name.Value == UpdatedSubnetName);
        _virtualNetworkRepository.Received(1).Update(Arg.Is<VirtualNetwork>(v => v.Id == _vnet.Id));
        _mapper.Received(1).Map<VirtualNetworkResult>(Arg.Any<VirtualNetwork>());
    }

    [Fact]
    public async Task Given_VirtualNetworkNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var command = new UpdateSubnetCommand(
            AzureResourceId.CreateUnique(),
            _subnetId,
            UpdatedSubnetName,
            AddressPrefix,
            null,
            null,
            ValidPrivateEndpointPolicy,
            null);

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
        var command = new UpdateSubnetCommand(
            _vnet.Id,
            _subnetId,
            UpdatedSubnetName,
            AddressPrefix,
            null,
            null,
            ValidPrivateEndpointPolicy,
            null);

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
        var command = new UpdateSubnetCommand(
            _vnet.Id,
            _subnetId,
            UpdatedSubnetName,
            AddressPrefix,
            null,
            null,
            ValidPrivateEndpointPolicy,
            null);

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
        _virtualNetworkRepository.DidNotReceive().Update(Arg.Any<VirtualNetwork>());
    }

    [Fact]
    public async Task Given_ValidSubnetId_When_Handle_Then_CorrectSubnetIsUpdatedAsync()
    {
        // Arrange
        var command = new UpdateSubnetCommand(
            _vnet.Id,
            _subnetId,
            UpdatedSubnetName,
            "10.0.2.0/24",
            null,
            null,
            ValidPrivateEndpointPolicy,
            null);

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
        var updatedSubnet = _vnet.Subnets.Single(s => s.Id.Value == _subnetId);
        updatedSubnet.Name.Value.Should().Be(UpdatedSubnetName);
        updatedSubnet.AddressPrefix.Should().Be("10.0.2.0/24");
    }

    [Fact]
    public async Task Given_ValidDelegation_When_Handle_Then_SubnetDelegationIsUpdatedAsync()
    {
        // Arrange
        var command = new UpdateSubnetCommand(
            _vnet.Id,
            _subnetId,
            UpdatedSubnetName,
            AddressPrefix,
            ValidDelegation,
            null,
            ValidPrivateEndpointPolicy,
            null);

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
        var updatedSubnet = _vnet.Subnets.Single(s => s.Id.Value == _subnetId);
        updatedSubnet.Delegation.Should().NotBeNull();
        updatedSubnet.Delegation!.Value.Should().Be(SubnetDelegation.Delegation.AppEnvironments);
    }

    [Fact]
    public async Task Given_UnknownSubnetId_When_Handle_Then_ThrowsInvalidOperationExceptionAsync()
    {
        // Arrange
        var unknownSubnetId = Guid.NewGuid();
        var command = new UpdateSubnetCommand(
            _vnet.Id,
            unknownSubnetId,
            UpdatedSubnetName,
            AddressPrefix,
            null,
            null,
            ValidPrivateEndpointPolicy,
            null);

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
