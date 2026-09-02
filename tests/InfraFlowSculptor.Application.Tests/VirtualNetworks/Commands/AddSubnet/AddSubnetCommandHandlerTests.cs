using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.AddSubnet;
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

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Commands.AddSubnet;

public sealed class AddSubnetCommandHandlerTests
{
    private const string SubnetName = "subnet-app";
    private const string AddressPrefix = "10.0.1.0/24";
    private const string ValidDelegation = "WebServerFarms";
    private const string ValidPrivateEndpointPolicy = "Disabled";

    private readonly IVirtualNetworkRepository _virtualNetworkRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly VirtualNetwork _vnet;
    private readonly AddSubnetCommandHandler _sut;

    public AddSubnetCommandHandlerTests()
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

        _sut = new AddSubnetCommandHandler(
            _virtualNetworkRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_AddsSubnetAndReturnsResultAsync()
    {
        // Arrange
        var command = new AddSubnetCommand(
            _vnet.Id,
            SubnetName,
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
        _virtualNetworkRepository.Received(1).Update(Arg.Is<VirtualNetwork>(v => v.Id == _vnet.Id));
        _mapper.Received(1).Map<VirtualNetworkResult>(Arg.Any<VirtualNetwork>());
    }

    [Fact]
    public async Task Given_VirtualNetworkNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var command = new AddSubnetCommand(
            AzureResourceId.CreateUnique(),
            SubnetName,
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
        var command = new AddSubnetCommand(
            _vnet.Id,
            SubnetName,
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
        var command = new AddSubnetCommand(
            _vnet.Id,
            SubnetName,
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
    public async Task Given_NullDelegation_When_Handle_Then_SucceedsWithoutDelegationAsync()
    {
        // Arrange
        var command = new AddSubnetCommand(
            _vnet.Id,
            SubnetName,
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
        _vnet.Subnets.Should().ContainSingle(s => s.Name.Value == SubnetName);
    }

    [Fact]
    public async Task Given_ValidDelegation_When_Handle_Then_SubnetHasDelegationAsync()
    {
        // Arrange
        var command = new AddSubnetCommand(
            _vnet.Id,
            "subnet-apps",
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
        _vnet.Subnets.Should().ContainSingle(s =>
            s.Name.Value == "subnet-apps" &&
            s.Delegation != null &&
            s.Delegation.Value == SubnetDelegation.Delegation.WebServerFarms);
    }

    [Fact]
    public async Task Given_ValidPrivateEndpointPolicy_When_Handle_Then_SubnetHasPolicyAsync()
    {
        // Arrange
        const string policyValue = "Enabled";
        var command = new AddSubnetCommand(
            _vnet.Id,
            "subnet-pe",
            AddressPrefix,
            null,
            null,
            policyValue,
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
        _vnet.Subnets.Should().ContainSingle(s =>
            s.Name.Value == "subnet-pe" &&
            s.PrivateEndpointNetworkPolicies.Value == PrivateEndpointNetworkPolicy.Policy.Enabled);
    }
}
