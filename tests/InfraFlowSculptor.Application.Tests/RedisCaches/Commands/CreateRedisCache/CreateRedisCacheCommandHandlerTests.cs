using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Common;
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

namespace InfraFlowSculptor.Application.Tests.RedisCaches.Commands.CreateRedisCache;

public sealed class CreateRedisCacheCommandHandlerTests
{
    private const string RedisCacheName = "redis-shared";
    private const string DevelopmentEnvironment = "dev";
    private const string ProductionEnvironment = "prod";
    private const string PremiumSku = "Premium";
    private const string AllKeysLruPolicy = "AllKeysLru";

    private readonly IRedisCacheRepository _redisCacheRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateRedisCacheCommand _command;
    private readonly CreateRedisCacheCommandHandler _sut;

    public CreateRedisCacheCommandHandlerTests()
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
        _command = new CreateRedisCacheCommand(
            _resourceGroup.Id,
            new Name(RedisCacheName),
            new Location(Location.LocationEnum.FranceCentral),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: null,
            DisableAccessKeyAuthentication: false,
            EnableAadAuth: false);
        _redisCacheRepository.Add(Arg.Any<RedisCache>())
            .Returns(callInfo => (RedisCache)callInfo.Args()[0]);
        _sut = new CreateRedisCacheCommandHandler(
            _redisCacheRepository, _resourceGroupRepository, _accessService, _mapper);
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
        _redisCacheRepository.DidNotReceive().Add(Arg.Any<RedisCache>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsRedisCacheAndMapsResultAsync()
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
        _redisCacheRepository.Received(1).Add(Arg.Is<RedisCache>(rc =>
            rc.ResourceGroupId == _resourceGroup.Id && rc.Name.Value == RedisCacheName));
        _mapper.Received(1).Map<RedisCacheResult>(Arg.Any<RedisCache>());
    }

    [Fact]
    public async Task Given_InvalidMinimumTlsVersion_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var invalidCommand = _command with { MinimumTlsVersion = "not-a-tls-version" };

        // Act
        var result = await _sut.Handle(invalidCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        _redisCacheRepository.DidNotReceive().Add(Arg.Any<RedisCache>());
    }

    [Fact]
    public async Task Given_EnvironmentSettings_When_Handle_Then_PersistsParsedOverridesAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var command = _command with
        {
            MinimumTlsVersion = "Tls12",
            EnvironmentSettings =
            [
                new RedisCacheEnvironmentConfigData(DevelopmentEnvironment, PremiumSku, 3, AllKeysLruPolicy),
                new RedisCacheEnvironmentConfigData(ProductionEnvironment, null, null, null),
            ],
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _redisCacheRepository.Received(1).Add(Arg.Is<RedisCache>(redisCache =>
            redisCache.MinimumTlsVersion != null
            && redisCache.MinimumTlsVersion.Value == TlsVersion.Version.Tls12
            && redisCache.EnvironmentSettings.Count == 2
            && redisCache.EnvironmentSettings.Any(setting =>
                setting.EnvironmentName == DevelopmentEnvironment
                && setting.Sku != null
                && setting.Sku.Value == RedisCacheSku.Sku.Premium
                && setting.Capacity == 3
                && setting.MaxMemoryPolicy != null
                && setting.MaxMemoryPolicy.Value == MaxMemoryPolicy.Policy.AllKeysLru)
            && redisCache.EnvironmentSettings.Any(setting =>
                setting.EnvironmentName == ProductionEnvironment
                && setting.Sku == null
                && setting.Capacity == null
                && setting.MaxMemoryPolicy == null)));
    }

    [Fact]
    public async Task Given_InvalidEnvironmentSku_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var command = _command with
        {
            EnvironmentSettings =
            [
                new RedisCacheEnvironmentConfigData(DevelopmentEnvironment, "UnsupportedSku", 1, null),
            ],
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("RedisCache.InvalidSku");
        _redisCacheRepository.DidNotReceive().Add(Arg.Any<RedisCache>());
    }

    [Fact]
    public async Task Given_InvalidEnvironmentMaxMemoryPolicy_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var command = _command with
        {
            EnvironmentSettings =
            [
                new RedisCacheEnvironmentConfigData(DevelopmentEnvironment, PremiumSku, 1, "UnsupportedPolicy"),
            ],
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("RedisCache.InvalidMaxMemoryPolicy");
        _redisCacheRepository.DidNotReceive().Add(Arg.Any<RedisCache>());
    }
}
