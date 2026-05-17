using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateEndpoints.Commands.UpdatePrivateEndpoint;
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

namespace InfraFlowSculptor.Application.Tests.PrivateEndpoints.Commands.UpdatePrivateEndpoint;

public sealed class UpdatePrivateEndpointCommandHandlerTests
{
    private readonly IAzureResourceRepository _resourceRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly KeyVault _existingResource;
    private readonly PrivateEndpointConfigId _configId;
    private readonly AzureResourceId _subnetId;
    private readonly UpdatePrivateEndpointCommand _command;
    private readonly UpdatePrivateEndpointCommandHandler _sut;

    public UpdatePrivateEndpointCommandHandlerTests()
    {
        _resourceRepository = Substitute.For<IAzureResourceRepository>();
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

        // Add a private endpoint so the handler can update it
        var pe = _existingResource.AddPrivateEndpoint(
            _subnetId, "vault", autoApproval: true, privateDnsZoneId: null, customNetworkInterfaceName: null);
        _configId = pe.Id;

        _command = new UpdatePrivateEndpointCommand(
            _existingResource.Id,
            _configId,
            _subnetId,
            GroupId: "vault",
            AutoApproval: false);
        _sut = new UpdatePrivateEndpointCommandHandler(_resourceRepository, _accessService, _mapper);
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
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange — set up the ResourceGroup navigation property via the NSubstitute mock
        SetupResourceWithNavigation();
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_InvalidGroupId_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        SetupResourceWithNavigation();
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var badCommand = _command with { GroupId = "invalidGroupId" };

        // Act
        var result = await _sut.Handle(badCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_UpdatesConfigAndMapsResultAsync()
    {
        // Arrange
        SetupResourceWithNavigation();
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<PrivateEndpointConfigResult>(Arg.Any<object>());
    }

    private void SetupResourceWithNavigation()
    {
        // The handler accesses resource.ResourceGroup!.InfraConfigId, so we need the navigation property set.
        // We use a real KeyVault with a private endpoint and set the ResourceGroup via reflection
        // because it's a protected setter on AzureResource.
        var rgProp = typeof(AzureResource).GetProperty(nameof(AzureResource.ResourceGroup));
        rgProp!.SetValue(_existingResource, _resourceGroup);

        // ResourceType is set by EF Core infrastructure, not domain Create. Set via reflection.
        var rtProp = typeof(AzureResource).GetProperty(nameof(AzureResource.ResourceType));
        rtProp!.SetValue(_existingResource, new ResourceTypeName("KeyVault"));

        _resourceRepository.GetByIdWithPrivateEndpointsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_existingResource);
    }
}
