using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateDnsZones.Commands.CreatePrivateDnsZone;
using InfraFlowSculptor.Application.PrivateDnsZones.Common;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.PrivateDnsZones.Commands.CreatePrivateDnsZone;

public sealed class CreatePrivateDnsZoneCommandHandlerTests
{
    private const string DnsZoneName = "privatelink.database.windows.net";

    private readonly IPrivateDnsZoneRepository _privateDnsZoneRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreatePrivateDnsZoneCommand _command;
    private readonly CreatePrivateDnsZoneCommandHandler _sut;

    public CreatePrivateDnsZoneCommandHandlerTests()
    {
        _privateDnsZoneRepository = Substitute.For<IPrivateDnsZoneRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _command = new CreatePrivateDnsZoneCommand(
            _resourceGroup.Id,
            new Name(DnsZoneName),
            new Location(Location.LocationEnum.FranceCentral));
        _privateDnsZoneRepository.Add(Arg.Any<PrivateDnsZone>())
            .Returns(callInfo => (PrivateDnsZone)callInfo.Args()[0]);
        _sut = new CreatePrivateDnsZoneCommandHandler(
            _privateDnsZoneRepository, _resourceGroupRepository, _accessService, _mapper);
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
        _privateDnsZoneRepository.DidNotReceive().Add(Arg.Any<PrivateDnsZone>());
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
        _privateDnsZoneRepository.DidNotReceive().Add(Arg.Any<PrivateDnsZone>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsPrivateDnsZoneAndMapsResultAsync()
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
        _privateDnsZoneRepository.Received(1).Add(Arg.Is<PrivateDnsZone>(zone =>
            zone.ResourceGroupId == _resourceGroup.Id && zone.Name.Value == DnsZoneName));
        _mapper.Received(1).Map<PrivateDnsZoneResult>(Arg.Any<PrivateDnsZone>());
    }
}
