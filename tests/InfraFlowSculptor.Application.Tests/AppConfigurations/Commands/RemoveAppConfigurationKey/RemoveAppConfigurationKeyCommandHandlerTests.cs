using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.RemoveAppConfigurationKey;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Commands.RemoveAppConfigurationKey;

public sealed class RemoveAppConfigurationKeyCommandHandlerTests
{
    private readonly IAppConfigurationRepository _appConfigurationRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly AppConfiguration _appConfiguration;
    private readonly RemoveAppConfigurationKeyCommand _command;
    private readonly RemoveAppConfigurationKeyCommandHandler _sut;

    public RemoveAppConfigurationKeyCommandHandlerTests()
    {
        _appConfigurationRepository = Substitute.For<IAppConfigurationRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _appConfiguration = AppConfiguration.Create(
            _resourceGroup.Id,
            new Name("appconfig-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new RemoveAppConfigurationKeyCommand(
            _appConfiguration.Id,
            AppConfigurationKeyId.CreateUnique());
        _sut = new RemoveAppConfigurationKeyCommandHandler(
            _appConfigurationRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_AppConfigNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _appConfigurationRepository.GetByIdWithConfigurationKeysAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((AppConfiguration?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_KeyNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — no configuration key with this ID
        _appConfigurationRepository.GetByIdWithConfigurationKeysAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_appConfiguration);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — add a key first so that check passes
        _appConfiguration.AddStaticConfigurationKey("MyKey", "MyLabel", new Dictionary<string, string> { ["prod"] = "MyValue" });
        var keyId = _appConfiguration.ConfigurationKeys.First().Id;
        var command = new RemoveAppConfigurationKeyCommand(_appConfiguration.Id, keyId);

        _appConfigurationRepository.GetByIdWithConfigurationKeysAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_appConfiguration);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        _appConfiguration.AddStaticConfigurationKey("MyKey", "MyLabel", new Dictionary<string, string> { ["prod"] = "MyValue" });
        var keyId = _appConfiguration.ConfigurationKeys.First().Id;
        var command = new RemoveAppConfigurationKeyCommand(_appConfiguration.Id, keyId);

        _appConfigurationRepository.GetByIdWithConfigurationKeysAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_appConfiguration);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ReturnsDeletedAsync()
    {
        // Arrange
        _appConfiguration.AddStaticConfigurationKey("MyKey", "MyLabel", new Dictionary<string, string> { ["prod"] = "MyValue" });
        var keyId = _appConfiguration.ConfigurationKeys.First().Id;
        var command = new RemoveAppConfigurationKeyCommand(_appConfiguration.Id, keyId);

        _appConfigurationRepository.GetByIdWithConfigurationKeysAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_appConfiguration);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        _appConfigurationRepository.Received(1).Update(Arg.Any<AppConfiguration>());
    }
}
