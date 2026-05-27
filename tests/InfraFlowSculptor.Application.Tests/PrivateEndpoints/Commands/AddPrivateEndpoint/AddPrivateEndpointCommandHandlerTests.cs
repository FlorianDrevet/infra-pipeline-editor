using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateEndpoints.Commands.AddPrivateEndpoint;
using InfraFlowSculptor.Application.PrivateEndpoints.Common;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.PrivateEndpoints.Commands.AddPrivateEndpoint;

public sealed class AddPrivateEndpointCommandHandlerTests
{
    private readonly IAzureResourceRepository _resourceRepository;
    private readonly IVirtualNetworkRepository _virtualNetworkRepository;
    private readonly IPrivateDnsZoneRepository _privateDnsZoneRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly KeyVault _existingResource;
    private readonly AzureResourceId _subnetId;
    private readonly AddPrivateEndpointCommand _command;
    private readonly AddPrivateEndpointCommandHandler _sut;

    public AddPrivateEndpointCommandHandlerTests()
    {
        _resourceRepository = Substitute.For<IAzureResourceRepository>();
        _virtualNetworkRepository = Substitute.For<IVirtualNetworkRepository>();
        _privateDnsZoneRepository = Substitute.For<IPrivateDnsZoneRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingResource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-test"),
            new Location(Location.LocationEnum.FranceCentral),
            enableRbacAuthorization: true);
        _subnetId = AzureResourceId.CreateUnique();
        _command = new AddPrivateEndpointCommand(
            _existingResource.Id,
            _subnetId,
            GroupId: "vault");
        _sut = new AddPrivateEndpointCommandHandler(
            _resourceRepository, _virtualNetworkRepository, _privateDnsZoneRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _resourceRepository.GetByIdWithPrivateEndpointsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        SetupResourceWithNavigation();
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_SubnetNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        SetupResourceWithNavigation();
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _virtualNetworkRepository.SubnetExistsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_AddsPrivateEndpointAndMapsResultAsync()
    {
        // Arrange
        SetupResourceWithNavigation();
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _virtualNetworkRepository.SubnetExistsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<PrivateEndpointConfigResult>(Arg.Any<object>());
    }

    private void SetupResourceWithNavigation()
    {
        // The handler accesses resource.ResourceGroup!.InfraConfigId, so we need the navigation property set.
        // ResourceGroup and ResourceType are protected/private set — use reflection.
        typeof(AzureResource).GetProperty(nameof(AzureResource.ResourceGroup))!
            .SetValue(_existingResource, _resourceGroup);
        typeof(AzureResource).GetProperty(nameof(AzureResource.ResourceType))!
            .SetValue(_existingResource, new ResourceTypeName("KeyVault"));

        _resourceRepository.GetByIdWithPrivateEndpointsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_existingResource);
    }
}
