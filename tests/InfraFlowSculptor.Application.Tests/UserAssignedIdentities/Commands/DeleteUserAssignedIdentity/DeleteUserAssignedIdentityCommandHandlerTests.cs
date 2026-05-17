using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.DeleteUserAssignedIdentity;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.UserAssignedIdentities.Commands.DeleteUserAssignedIdentity;

public sealed class DeleteUserAssignedIdentityCommandHandlerTests
{
    private readonly IUserAssignedIdentityRepository _userAssignedIdentityRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly UserAssignedIdentity _userAssignedIdentity;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteUserAssignedIdentityCommand _command;
    private readonly DeleteUserAssignedIdentityCommandHandler _sut;

    public DeleteUserAssignedIdentityCommandHandlerTests()
    {
        _userAssignedIdentityRepository = Substitute.For<IUserAssignedIdentityRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _userAssignedIdentity = UserAssignedIdentity.Create(
            _resourceGroup.Id,
            new Name("uai-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new DeleteUserAssignedIdentityCommand(_userAssignedIdentity.Id);
        _sut = new DeleteUserAssignedIdentityCommandHandler(
            _userAssignedIdentityRepository,
            _resourceGroupRepository,
            _azureResourceRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_UserAssignedIdentityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _userAssignedIdentityRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((UserAssignedIdentity?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _userAssignedIdentityRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
        await _azureResourceRepository.DidNotReceive().RevertRoleAssignmentsToSystemAssignedAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _userAssignedIdentityRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_userAssignedIdentity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _userAssignedIdentityRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
        await _azureResourceRepository.DidNotReceive().RevertRoleAssignmentsToSystemAssignedAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_RevertsRoleAssignmentsAndDeletesAsync()
    {
        // Arrange
        _userAssignedIdentityRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_userAssignedIdentity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _azureResourceRepository.RevertRoleAssignmentsToSystemAssignedAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(2);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _azureResourceRepository.Received(1).RevertRoleAssignmentsToSystemAssignedAsync(_command.Id, Arg.Any<CancellationToken>());
        await _userAssignedIdentityRepository.Received(1).DeleteAsync(_userAssignedIdentity.Id);
    }
}
