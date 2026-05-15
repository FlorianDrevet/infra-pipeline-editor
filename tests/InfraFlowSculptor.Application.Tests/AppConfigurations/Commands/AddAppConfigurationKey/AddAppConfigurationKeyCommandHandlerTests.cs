using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Commands.AddAppConfigurationKey;

public sealed class AddAppConfigurationKeyCommandHandlerTests
{
    private readonly IAppConfigurationRepository _appConfigurationRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IAddAppConfigurationKeyAdditionService _additionService;
    private readonly DomainInfrastructureConfig _infraConfig;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly AppConfiguration _appConfiguration;
    private readonly AddAppConfigurationKeyCommand _command;
    private readonly AddAppConfigurationKeyCommandHandler _sut;

    public AddAppConfigurationKeyCommandHandlerTests()
    {
        _appConfigurationRepository = Substitute.For<IAppConfigurationRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _additionService = Substitute.For<IAddAppConfigurationKeyAdditionService>();

        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _infraConfig = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-config"),
            _infraConfig.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _appConfiguration = AppConfiguration.Create(
            _resourceGroup.Id,
            new Name("appcs-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new AddAppConfigurationKeyCommand(
            _appConfiguration.Id,
            "Core:Authorization:ClientSecret",
            Label: "v1",
            EnvironmentValues: new Dictionary<string, string>
            {
                ["prod"] = "secret",
            });

        _sut = new AddAppConfigurationKeyCommandHandler(
            _appConfigurationRepository,
            _resourceGroupRepository,
            _accessService,
            _additionService);
    }

    [Fact]
    public async Task Given_ValidUniqueRequest_When_Handle_Then_DelegatesToAdditionServiceAsync()
    {
        // Arrange
        var expected = new AppConfigurationKeyResult(
            AppConfigurationKeyId.CreateUnique(),
            _appConfiguration.Id,
            _command.Key,
            _command.Label,
            _command.EnvironmentValues,
            SourceResourceId: null,
            SourceOutputName: null,
            IsOutputReference: false,
            KeyVaultResourceId: null,
            SecretName: null,
            IsKeyVaultReference: false,
            HasKeyVaultAccess: null,
            SecretValueAssignment: null,
            VariableGroupId: null,
            PipelineVariableName: null,
            VariableGroupName: null,
            IsViaVariableGroup: false);

        _appConfigurationRepository.GetByIdWithConfigurationKeysAsync(_appConfiguration.Id, Arg.Any<CancellationToken>())
            .Returns(_appConfiguration);
        _resourceGroupRepository.GetByIdAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_infraConfig.Id, Arg.Any<CancellationToken>())
            .Returns(_infraConfig);
        _additionService.AddAsync(_command, _appConfiguration, _infraConfig, Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expected);

        await _additionService.Received(1)
            .AddAsync(_command, _appConfiguration, _infraConfig, Arg.Any<CancellationToken>());
    }
}