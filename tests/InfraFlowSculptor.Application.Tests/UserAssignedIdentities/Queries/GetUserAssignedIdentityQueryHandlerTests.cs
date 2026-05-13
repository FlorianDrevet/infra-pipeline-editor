using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Application.UserAssignedIdentities.Queries.GetUserAssignedIdentity;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.UserAssignedIdentities.Queries;

public sealed class GetUserAssignedIdentityQueryHandlerTests
{
    private readonly IUserAssignedIdentityRepository _identityRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly UserAssignedIdentity _identity;
    private readonly GetUserAssignedIdentityQuery _query;
    private readonly GetUserAssignedIdentityQueryHandler _sut;

    public GetUserAssignedIdentityQueryHandlerTests()
    {
        _identityRepository = Substitute.For<IUserAssignedIdentityRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-identities"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _identity = UserAssignedIdentity.Create(
            _resourceGroup.Id,
            new Name("uai-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _query = new GetUserAssignedIdentityQuery(_identity.Id);
        _sut = new GetUserAssignedIdentityQueryHandler(
            _identityRepository,
            _resourceGroupRepository,
            _accessService,
            _mapper);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsNotFoundToHideExistenceAsync()
    {
        // Arrange
        _identityRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_identity);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_identity.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _identityRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_MapsResultUsingReadOnlyLookupsAsync()
    {
        // Arrange
        _identityRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_identity);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_identity.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<UserAssignedIdentityResult>(_identity);
        await _identityRepository.Received(1)
            .GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_identity.ResourceGroupId, Arg.Any<CancellationToken>());
        await _identityRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }
}