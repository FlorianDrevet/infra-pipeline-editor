using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UpdateUserAssignedIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.UserAssignedIdentities.Commands.UpdateUserAssignedIdentity;

public sealed class UpdateUserAssignedIdentityCommandHandlerTests
{
    private readonly IUserAssignedIdentityRepository _userAssignedIdentityRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly UserAssignedIdentity _existingEntity;
    private readonly UpdateUserAssignedIdentityCommand _command;
    private readonly UpdateUserAssignedIdentityCommandHandler _sut;

    public UpdateUserAssignedIdentityCommandHandlerTests()
    {
        _userAssignedIdentityRepository = Substitute.For<IUserAssignedIdentityRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = UserAssignedIdentity.Create(
            _resourceGroup.Id,
            new Name("uai-old"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateUserAssignedIdentityCommand(
            _existingEntity.Id,
            new Name("uai-renamed"),
            new Location(Location.LocationEnum.WestEurope));
        _userAssignedIdentityRepository.Update(Arg.Any<UserAssignedIdentity>())
            .Returns(callInfo => (UserAssignedIdentity)callInfo.Args()[0]);
        _sut = new UpdateUserAssignedIdentityCommandHandler(
            _userAssignedIdentityRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _userAssignedIdentityRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((UserAssignedIdentity?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _userAssignedIdentityRepository.DidNotReceive().Update(Arg.Any<UserAssignedIdentity>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _userAssignedIdentityRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _userAssignedIdentityRepository.DidNotReceive().Update(Arg.Any<UserAssignedIdentity>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _userAssignedIdentityRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _userAssignedIdentityRepository.Received(1).Update(Arg.Is<UserAssignedIdentity>(u =>
            u.Name.Value == "uai-renamed"));
        _mapper.Received(1).Map<UserAssignedIdentityResult>(Arg.Any<UserAssignedIdentity>());
    }
}
