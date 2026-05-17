using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.CreateNetworkSecurityGroup;
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

namespace InfraFlowSculptor.Application.Tests.NetworkSecurityGroups.Commands.CreateNetworkSecurityGroup;

public sealed class CreateNetworkSecurityGroupCommandHandlerTests
{
    private const string NsgName = "nsg-shared";

    private readonly INetworkSecurityGroupRepository _nsgRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateNetworkSecurityGroupCommand _command;
    private readonly CreateNetworkSecurityGroupCommandHandler _sut;

    public CreateNetworkSecurityGroupCommandHandlerTests()
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
        _command = new CreateNetworkSecurityGroupCommand(
            _resourceGroup.Id,
            new Name(NsgName),
            new Location(Location.LocationEnum.FranceCentral));
        _nsgRepository.Add(Arg.Any<NetworkSecurityGroup>())
            .Returns(callInfo => (NetworkSecurityGroup)callInfo.Args()[0]);
        _sut = new CreateNetworkSecurityGroupCommandHandler(
            _nsgRepository, _resourceGroupRepository, _accessService, _mapper);
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
        _nsgRepository.DidNotReceive().Add(Arg.Any<NetworkSecurityGroup>());
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
        _nsgRepository.DidNotReceive().Add(Arg.Any<NetworkSecurityGroup>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsNetworkSecurityGroupAndMapsResultAsync()
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
        _nsgRepository.Received(1).Add(Arg.Is<NetworkSecurityGroup>(nsg =>
            nsg.ResourceGroupId == _resourceGroup.Id && nsg.Name.Value == NsgName));
        _mapper.Received(1).Map<NetworkSecurityGroupResult>(Arg.Any<NetworkSecurityGroup>());
    }
}
