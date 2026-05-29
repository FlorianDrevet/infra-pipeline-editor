using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.NetworkingProfiles.Commands.RemovePrivateEndpointConfig;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;

namespace InfraFlowSculptor.Application.Tests.NetworkingProfiles.Commands.RemovePrivateEndpointConfig;

public sealed class RemovePrivateEndpointConfigCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly RemovePrivateEndpointConfigCommandHandler _sut;

    private readonly DomainInfrastructureConfig _config;
    private readonly KeyVault _resource;

    public RemovePrivateEndpointConfigCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        var projectId = ProjectId.CreateUnique();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), projectId);
        var resourceGroup = DomainResourceGroup.Create(
            new Name("rg-app"), _config.Id, new Location(Location.LocationEnum.FranceCentral));
        _resource = KeyVault.Create(resourceGroup.Id, new Name("kv-test"), new Location(Location.LocationEnum.FranceCentral));

        // Pre-configure resource with a PE config
        var peConfig = PrivateEndpointConfiguration.AutoManaged(AzureResourceId.CreateUnique(), new Name("snet-pe"));
        _resource.ConfigurePrivateEndpoint(peConfig);

        _sut = new RemovePrivateEndpointConfigCommandHandler(_azureResourceRepository, _accessService);
    }

    [Fact]
    public async Task Given_PrivatizedResource_When_Handle_Then_RemovesConfigAsync()
    {
        // Arrange
        var command = new RemovePrivateEndpointConfigCommand(_config.Id, _resource.Id);

        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _azureResourceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_resource);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _resource.IsPrivatized.Should().BeFalse();
        _resource.PrivateEndpointConfiguration.Should().BeNull();
        _azureResourceRepository.Received(1).Update(_resource);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new RemovePrivateEndpointConfigCommand(_config.Id, _resource.Id);

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
        var command = new RemovePrivateEndpointConfigCommand(_config.Id, _resource.Id);

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
}
