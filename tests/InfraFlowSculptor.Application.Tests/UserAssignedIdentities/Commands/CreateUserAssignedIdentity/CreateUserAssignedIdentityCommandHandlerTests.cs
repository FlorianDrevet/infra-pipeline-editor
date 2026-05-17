using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;

public sealed class CreateUserAssignedIdentityCommandHandlerTests
{
    private const string IdentityName = "id-shared";

    private readonly IUserAssignedIdentityRepository _userAssignedIdentityRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateUserAssignedIdentityCommand _command;
    private readonly CreateUserAssignedIdentityCommandHandler _sut;

    public CreateUserAssignedIdentityCommandHandlerTests()
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
        _command = new CreateUserAssignedIdentityCommand(
            _resourceGroup.Id,
            new Name(IdentityName),
            new Location(Location.LocationEnum.FranceCentral));
        _userAssignedIdentityRepository.Add(Arg.Any<UserAssignedIdentity>())
            .Returns(callInfo => (UserAssignedIdentity)callInfo.Args()[0]);
        _sut = new CreateUserAssignedIdentityCommandHandler(
            _userAssignedIdentityRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _userAssignedIdentityRepository.DidNotReceive().Add(Arg.Any<UserAssignedIdentity>());
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _userAssignedIdentityRepository.DidNotReceive().Add(Arg.Any<UserAssignedIdentity>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUserAssignedIdentityAndMapsResultAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _userAssignedIdentityRepository.Received(1).Add(Arg.Is<UserAssignedIdentity>(id =>
            id.ResourceGroupId == _resourceGroup.Id && id.Name.Value == IdentityName));
        _mapper.Received(1).Map<UserAssignedIdentityResult>(Arg.Any<UserAssignedIdentity>());
    }
}
