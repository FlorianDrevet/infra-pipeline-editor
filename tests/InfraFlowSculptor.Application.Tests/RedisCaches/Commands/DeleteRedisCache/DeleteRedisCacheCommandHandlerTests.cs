using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Commands.DeleteRedisCache;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.RedisCaches.Commands.DeleteRedisCache;

public sealed class DeleteRedisCacheCommandHandlerTests
{
    private readonly IRedisCacheRepository _redisCacheRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly RedisCache _redisCache;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteRedisCacheCommand _command;
    private readonly DeleteRedisCacheCommandHandler _sut;

    public DeleteRedisCacheCommandHandlerTests()
    {
        _redisCacheRepository = Substitute.For<IRedisCacheRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _redisCache = RedisCache.Create(
            _resourceGroup.Id,
            new Name("redis-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            redisVersion: 6,
            enableNonSslPort: false,
            minimumTlsVersion: null,
            disableAccessKeyAuthentication: false,
            enableAadAuth: false);
        _command = new DeleteRedisCacheCommand(_redisCache.Id);
        _sut = new DeleteRedisCacheCommandHandler(
            _redisCacheRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_RedisCacheNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _redisCacheRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((RedisCache?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _redisCacheRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesRedisCacheAsync()
    {
        // Arrange
        _redisCacheRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_redisCache);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _redisCacheRepository.Received(1).DeleteAsync(_redisCache.Id);
    }
}
