using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UnlinkResourceFromIdentity;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.UserAssignedIdentities.Commands.UnlinkResourceFromIdentity;

public sealed class UnlinkResourceFromIdentityCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly UnlinkResourceFromIdentityCommandHandler _sut;

    public UnlinkResourceFromIdentityCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sut = new UnlinkResourceFromIdentityCommandHandler(
            _azureResourceRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_IdentityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var command = new UnlinkResourceFromIdentityCommand(
            AzureResourceId.CreateUnique(), AzureResourceId.CreateUnique());
        _azureResourceRepository.GetByIdWithRoleAssignmentsAsync(command.IdentityId, Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var identity = KeyVault.Create(
            _resourceGroup.Id,
            new Name("uai-test"),
            new Location(Location.LocationEnum.FranceCentral));
        var command = new UnlinkResourceFromIdentityCommand(
            identity.Id, AzureResourceId.CreateUnique());

        _azureResourceRepository.GetByIdWithRoleAssignmentsAsync(identity.Id, Arg.Any<CancellationToken>())
            .Returns(identity);
        _resourceGroupRepository.GetByIdAsync(identity.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsForbiddenAsync()
    {
        // Arrange
        var identity = KeyVault.Create(
            _resourceGroup.Id,
            new Name("uai-test"),
            new Location(Location.LocationEnum.FranceCentral));
        var command = new UnlinkResourceFromIdentityCommand(
            identity.Id, AzureResourceId.CreateUnique());

        _azureResourceRepository.GetByIdWithRoleAssignmentsAsync(identity.Id, Arg.Any<CancellationToken>())
            .Returns(identity);
        _resourceGroupRepository.GetByIdAsync(identity.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Given_SourceResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var identity = KeyVault.Create(
            _resourceGroup.Id,
            new Name("uai-test"),
            new Location(Location.LocationEnum.FranceCentral));
        var sourceResourceId = AzureResourceId.CreateUnique();
        var command = new UnlinkResourceFromIdentityCommand(identity.Id, sourceResourceId);

        _azureResourceRepository.GetByIdWithRoleAssignmentsAsync(identity.Id, Arg.Any<CancellationToken>())
            .Returns(identity);
        _resourceGroupRepository.GetByIdAsync(identity.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _azureResourceRepository.GetByIdWithRoleAssignmentsAsync(sourceResourceId, Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_NoAssignmentsToMove_When_Handle_Then_ReturnsDeletedAsync()
    {
        // Arrange
        var identity = KeyVault.Create(
            _resourceGroup.Id,
            new Name("uai-test"),
            new Location(Location.LocationEnum.FranceCentral));
        var sourceResource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-source"),
            new Location(Location.LocationEnum.FranceCentral));
        var command = new UnlinkResourceFromIdentityCommand(identity.Id, sourceResource.Id);

        _azureResourceRepository.GetByIdWithRoleAssignmentsAsync(identity.Id, Arg.Any<CancellationToken>())
            .Returns(identity);
        _resourceGroupRepository.GetByIdAsync(identity.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _azureResourceRepository.GetByIdWithRoleAssignmentsAsync(sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(sourceResource);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
    }
}
