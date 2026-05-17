using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;

public sealed class UpdateRoleAssignmentIdentityCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IRoleAssignmentDomainService _roleAssignmentDomainService;
    private readonly KeyVault _resource;
    private readonly UpdateRoleAssignmentIdentityCommandHandler _sut;

    public UpdateRoleAssignmentIdentityCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _roleAssignmentDomainService = Substitute.For<IRoleAssignmentDomainService>();
        var rgId = ResourceGroupId.CreateUnique();
        _resource = KeyVault.Create(rgId, new Name("kv-test"),
            new Location(Location.LocationEnum.WestEurope));
        _sut = new UpdateRoleAssignmentIdentityCommandHandler(
            _azureResourceRepository, _roleAssignmentDomainService);
    }

    [Fact]
    public async Task Given_AuthorizationFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new UpdateRoleAssignmentIdentityCommand(
            _resource.Id, RoleAssignmentId.CreateUnique(), "SystemAssigned", null);

        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                Arg.Any<AzureResourceId>(), true, Arg.Any<CancellationToken>())
            .Returns(Error.Forbidden("Forbidden", "Access denied"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_IdentityValidationFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new UpdateRoleAssignmentIdentityCommand(
            _resource.Id, RoleAssignmentId.CreateUnique(), "InvalidType", null);

        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                Arg.Any<AzureResourceId>(), true, Arg.Any<CancellationToken>())
            .Returns(_resource);

        _roleAssignmentDomainService.ValidateIdentityTypeAsync(
                "InvalidType", null, Arg.Any<CancellationToken>())
            .Returns(Error.Validation("InvalidIdentityType", "Invalid identity type"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_RoleAssignmentNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var managedIdentityType = new ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum.SystemAssigned);
        var command = new UpdateRoleAssignmentIdentityCommand(
            _resource.Id, RoleAssignmentId.CreateUnique(), "SystemAssigned", null);

        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                Arg.Any<AzureResourceId>(), true, Arg.Any<CancellationToken>())
            .Returns(_resource);

        _roleAssignmentDomainService.ValidateIdentityTypeAsync(
                "SystemAssigned", null, Arg.Any<CancellationToken>())
            .Returns(managedIdentityType);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
