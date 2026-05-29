using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.NetworkingProfiles.Commands.SetPrivateEndpointConfig;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;

namespace InfraFlowSculptor.Application.Tests.NetworkingProfiles.Commands.SetPrivateEndpointConfig;

public sealed class SetPrivateEndpointConfigCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IVirtualNetworkRepository _virtualNetworkRepository;
    private readonly IInfrastructureConfigRepository _infraConfigRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly SetPrivateEndpointConfigCommandHandler _sut;

    private readonly DomainInfrastructureConfig _config;
    private readonly DomainInfrastructureConfig _otherConfig;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainResourceGroup _vnetResourceGroup;
    private readonly KeyVault _resource;
    private readonly VirtualNetwork _vnet;

    public SetPrivateEndpointConfigCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _virtualNetworkRepository = Substitute.For<IVirtualNetworkRepository>();
        _infraConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        var projectId = ProjectId.CreateUnique();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), projectId);
        _otherConfig = DomainInfrastructureConfig.Create(new Name("secondary"), projectId);

        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-app"), _config.Id, new Location(Location.LocationEnum.FranceCentral));
        _vnetResourceGroup = DomainResourceGroup.Create(
            new Name("rg-network"), _otherConfig.Id, new Location(Location.LocationEnum.FranceCentral));

        _resource = KeyVault.Create(_resourceGroup.Id, new Name("kv-test"), new Location(Location.LocationEnum.FranceCentral));
        _vnet = VirtualNetwork.Create(_vnetResourceGroup.Id, new Name("vnet-main"), new Location(Location.LocationEnum.FranceCentral));
        _vnet.AddSubnet(new Name("snet-pe"), null, [], new PrivateEndpointNetworkPolicy(PrivateEndpointNetworkPolicy.Policy.Disabled), null);

        _sut = new SetPrivateEndpointConfigCommandHandler(
            _azureResourceRepository,
            _resourceGroupRepository,
            _virtualNetworkRepository,
            _infraConfigRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_ConfiguresPrivateEndpointAsync()
    {
        // Arrange
        var command = new SetPrivateEndpointConfigCommand(
            _config.Id, _resource.Id, _vnet.Id, "snet-pe", "AutoManaged", null, null);

        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _azureResourceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByContainedResourceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_vnetResourceGroup);
        _infraConfigRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_otherConfig);
        _virtualNetworkRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _resource.IsPrivatized.Should().BeTrue();
        _resource.PrivateEndpointConfiguration.Should().NotBeNull();
        _resource.PrivateEndpointConfiguration!.VirtualNetworkId.Should().Be(_vnet.Id);
        _resource.PrivateEndpointConfiguration.SubnetName.Value.Should().Be("snet-pe");
        _azureResourceRepository.Received(1).Update(_resource);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new SetPrivateEndpointConfigCommand(
            _config.Id, _resource.Id, _vnet.Id, "snet-pe", "AutoManaged", null, null);

        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Forbidden());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Given_ResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var command = new SetPrivateEndpointConfigCommand(
            _config.Id, _resource.Id, _vnet.Id, "snet-pe", "AutoManaged", null, null);

        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _azureResourceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_VNetNotInSameProject_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var foreignProject = ProjectId.CreateUnique();
        var foreignConfig = DomainInfrastructureConfig.Create(new Name("foreign"), foreignProject);
        var command = new SetPrivateEndpointConfigCommand(
            _config.Id, _resource.Id, _vnet.Id, "snet-pe", "AutoManaged", null, null);

        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _azureResourceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByContainedResourceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_vnetResourceGroup);
        _infraConfigRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(foreignConfig);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Contain("NotInSameProject");
    }

    [Fact]
    public async Task Given_SubnetNotFound_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new SetPrivateEndpointConfigCommand(
            _config.Id, _resource.Id, _vnet.Id, "snet-nonexistent", "AutoManaged", null, null);

        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _azureResourceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByContainedResourceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_vnetResourceGroup);
        _infraConfigRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_otherConfig);
        _virtualNetworkRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Contain("Subnet");
    }

    [Fact]
    public async Task Given_ExistingHubMode_When_Handle_Then_ConfiguresWithHubFieldsAsync()
    {
        // Arrange
        var command = new SetPrivateEndpointConfigCommand(
            _config.Id, _resource.Id, _vnet.Id, "snet-pe",
            "ExistingHub", "rg-dns-hub", "sub-dns-hub");

        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _azureResourceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByContainedResourceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_vnetResourceGroup);
        _infraConfigRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_otherConfig);
        _virtualNetworkRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_vnet);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _resource.PrivateEndpointConfiguration!.DnsMode.Value.Should().Be(PrivateEndpointDnsMode.Mode.ExistingHub);
        _resource.PrivateEndpointConfiguration.DnsHubResourceGroupId.Should().Be("rg-dns-hub");
        _resource.PrivateEndpointConfiguration.DnsHubSubscriptionId.Should().Be("sub-dns-hub");
    }
}
