using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.RedisCaches.Commands.UpdateRedisCache;

public sealed class UpdateRedisCacheCommandHandlerTests
{
    private readonly IRedisCacheRepository _redisCacheRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly RedisCache _existingRedisCache;
    private readonly UpdateRedisCacheCommandHandler _sut;

    public UpdateRedisCacheCommandHandlerTests()
    {
        _redisCacheRepository = Substitute.For<IRedisCacheRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingRedisCache = RedisCache.Create(
            _resourceGroup.Id,
            new Name("redis-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            redisVersion: 6,
            enableNonSslPort: false,
            minimumTlsVersion: new TlsVersion(TlsVersion.Version.Tls12),
            disableAccessKeyAuthentication: false,
            enableAadAuth: false);
        _redisCacheRepository.UpdateAsync(Arg.Any<RedisCache>())
            .Returns(callInfo => Task.FromResult((RedisCache)callInfo.Args()[0]));
        _sut = new UpdateRedisCacheCommandHandler(
            _redisCacheRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    private UpdateRedisCacheCommand BuildCommand(string? minimumTlsVersion = "Tls12") =>
        new(
            _existingRedisCache.Id,
            new Name("redis-renamed"),
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 7,
            EnableNonSslPort: true,
            MinimumTlsVersion: minimumTlsVersion,
            DisableAccessKeyAuthentication: true,
            EnableAadAuth: true);

    [Fact]
    public async Task Given_RedisCacheNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _redisCacheRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((RedisCache?)null);

        // Act
        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _redisCacheRepository.DidNotReceive().UpdateAsync(Arg.Any<RedisCache>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _redisCacheRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingRedisCache);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _redisCacheRepository.DidNotReceive().UpdateAsync(Arg.Any<RedisCache>());
    }

    [Fact]
    public async Task Given_InvalidMinimumTlsVersion_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        _redisCacheRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingRedisCache);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(BuildCommand(minimumTlsVersion: "NotAVersion"), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        await _redisCacheRepository.DidNotReceive().UpdateAsync(Arg.Any<RedisCache>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedRedisCacheAndMapsResultAsync()
    {
        // Arrange
        _redisCacheRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingRedisCache);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _redisCacheRepository.Received(1).UpdateAsync(Arg.Is<RedisCache>(rc =>
            rc.Name.Value == "redis-renamed"
            && rc.RedisVersion == 7
            && rc.EnableNonSslPort == true));
        _mapper.Received(1).Map<RedisCacheResult>(Arg.Any<RedisCache>());
    }
}
