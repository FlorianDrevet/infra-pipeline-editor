using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.UpdateNetworkSecurityGroup;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Common;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.NetworkSecurityGroups.Commands.UpdateNetworkSecurityGroup;

public sealed class UpdateNetworkSecurityGroupCommandHandlerTests
{
    private readonly INetworkSecurityGroupRepository _nsgRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly NetworkSecurityGroup _existingEntity;
    private readonly UpdateNetworkSecurityGroupCommand _command;
    private readonly UpdateNetworkSecurityGroupCommandHandler _sut;

    public UpdateNetworkSecurityGroupCommandHandlerTests()
    {
        _nsgRepository = Substitute.For<INetworkSecurityGroupRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = NetworkSecurityGroup.Create(
            _resourceGroup.Id,
            new Name("nsg-old"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateNetworkSecurityGroupCommand(
            _existingEntity.Id,
            new Name("nsg-renamed"),
            new Location(Location.LocationEnum.WestEurope));
        _nsgRepository.Update(Arg.Any<NetworkSecurityGroup>())
            .Returns(callInfo => (NetworkSecurityGroup)callInfo.Args()[0]);
        _sut = new UpdateNetworkSecurityGroupCommandHandler(
            _nsgRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _nsgRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((NetworkSecurityGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _nsgRepository.DidNotReceive().Update(Arg.Any<NetworkSecurityGroup>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _nsgRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _nsgRepository.DidNotReceive().Update(Arg.Any<NetworkSecurityGroup>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _nsgRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _nsgRepository.Received(1).Update(Arg.Is<NetworkSecurityGroup>(e =>
            e.Name.Value == "nsg-renamed"));
        _mapper.Received(1).Map<NetworkSecurityGroupResult>(Arg.Any<NetworkSecurityGroup>());
    }
}
