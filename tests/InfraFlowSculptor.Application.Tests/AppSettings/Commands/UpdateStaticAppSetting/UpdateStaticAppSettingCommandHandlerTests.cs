using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppSettings.Commands.UpdateStaticAppSetting;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppSettings.Commands.UpdateStaticAppSetting;

public sealed class UpdateStaticAppSettingCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly WebApp _existingResource;
    private readonly AppSettingId _appSettingId;
    private readonly UpdateStaticAppSettingCommand _command;
    private readonly UpdateStaticAppSettingCommandHandler _sut;

    public UpdateStaticAppSettingCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingResource = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-settings"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            "8.0",
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null);

        // Add a static app setting so the handler can find and update it
        var setting = _existingResource.AddStaticAppSetting("MY_VAR",
            new Dictionary<string, string> { ["dev"] = "val1" });
        _appSettingId = setting.Id;

        _command = new UpdateStaticAppSettingCommand(
            _existingResource.Id,
            _appSettingId,
            "MY_VAR_UPDATED",
            new Dictionary<string, string> { ["dev"] = "val2" });
        _sut = new UpdateStaticAppSettingCommandHandler(
            _azureResourceRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_ResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithAppSettingsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithAppSettingsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_existingResource);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AppSettingNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithAppSettingsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_existingResource);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var badCommand = _command with { AppSettingId = new AppSettingId(Guid.NewGuid()) };

        // Act
        var result = await _sut.Handle(badCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_DuplicateName_When_Handle_Then_ReturnsConflictAsync()
    {
        // Arrange — add a second setting so the duplicate name check triggers
        _existingResource.AddStaticAppSetting("DUPLICATE_NAME",
            new Dictionary<string, string> { ["dev"] = "x" });
        _azureResourceRepository.GetByIdWithAppSettingsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_existingResource);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var dupCommand = _command with { Name = "DUPLICATE_NAME" };

        // Act
        var result = await _sut.Handle(dupCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_UpdatesSettingAndReturnsResultAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithAppSettingsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_existingResource);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<AppSettingResult>();
        result.Value.Name.Should().Be("MY_VAR_UPDATED");
        _azureResourceRepository.Received(1).Update(Arg.Any<AzureResource>());
    }
}
