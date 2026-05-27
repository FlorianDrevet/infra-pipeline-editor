using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Commands.UnassignIdentityFromResource;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.UnassignIdentityFromResource;

public sealed class UnassignIdentityFromResourceCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IRoleAssignmentDomainService _roleAssignmentDomainService;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly UnassignIdentityFromResourceCommandHandler _sut;

    public UnassignIdentityFromResourceCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _roleAssignmentDomainService = Substitute.For<IRoleAssignmentDomainService>();
        var config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sut = new UnassignIdentityFromResourceCommandHandler(
            _azureResourceRepository, _roleAssignmentDomainService);
    }

    [Fact]
    public async Task Given_AuthorizationFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var resourceId = AzureResourceId.CreateUnique();
        var command = new UnassignIdentityFromResourceCommand(resourceId);
        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                resourceId, false, Arg.Any<CancellationToken>())
            .Returns(Error.Forbidden());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        _azureResourceRepository.DidNotReceive().Update(Arg.Any<AzureResource>());
    }

    [Fact]
    public async Task Given_ResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var resourceId = AzureResourceId.CreateUnique();
        var command = new UnassignIdentityFromResourceCommand(resourceId);
        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                resourceId, false, Arg.Any<CancellationToken>())
            .Returns(Error.NotFound());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_UnassignsIdentityAndReturnsSuccessAsync()
    {
        // Arrange
        var resource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-test"),
            new Location(Location.LocationEnum.FranceCentral));
        var identityId = AzureResourceId.CreateUnique();
        resource.AssignUserAssignedIdentity(identityId);

        var command = new UnassignIdentityFromResourceCommand(resource.Id);
        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                resource.Id, false, Arg.Any<CancellationToken>())
            .Returns(resource);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        resource.AssignedUserAssignedIdentityId.Should().BeNull();
        _azureResourceRepository.Received(1).Update(resource);
    }
}
