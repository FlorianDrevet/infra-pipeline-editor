using FluentAssertions;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.AppConfigurationAggregate;

public sealed class AppConfigurationTests
{
    private const string DefaultName = "appcs-prod";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const string DefaultKey = "Core:Authorization:ClientId";
    private const string DefaultLabel = "v1";
    private const string DefaultSecretName = "client-secret";
    private const string DefaultPipelineVariableName = "ClientId";
    private const string DefaultSourceOutputName = "primaryConnectionString";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static AppConfiguration CreateValid(bool isExisting = false)
    {
        return AppConfiguration.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();

        // Act
        var sut = AppConfiguration.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue));

        // Assert
        sut.Id.Should().NotBeNull();
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
        sut.ConfigurationKeys.Should().BeEmpty();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PopulatesCollection()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (string?)"Free", (int?)7, (bool?)false, (bool?)false, (string?)"Enabled"),
            (ProdEnvironment, (string?)"Standard", (int?)30, (bool?)true, (bool?)true, (string?)"Disabled"),
        };

        // Act
        var sut = AppConfiguration.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            environmentSettings: settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_IgnoresEnvironmentSettings()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (string?)"Free", (int?)7, (bool?)false, (bool?)false, (string?)"Enabled"),
        };

        // Act
        var sut = AppConfiguration.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            environmentSettings: settings,
            isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsNameAndLocation()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        sut.Update(new Name("appcs-updated"), new Location(Location.LocationEnum.NorthEurope));

        // Assert
        sut.Name.Value.Should().Be("appcs-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
    }

    // ─── SetEnvironmentSettings ────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "Standard", 30, true, true, "Disabled");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntry()
    {
        // Arrange
        var sut = CreateValid();
        sut.SetEnvironmentSettings(ProdEnvironment, "Free", 7, false, false, "Enabled");

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "Standard", 30, true, true, "Disabled");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().Sku.Should().Be("Standard");
    }

    [Fact]
    public void Given_IsExisting_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValid(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "Standard", 30, true, true, "Disabled");

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── AddStaticConfigurationKey ─────────────────────────────────────────

    [Fact]
    public void Given_StaticKey_When_AddStaticConfigurationKey_Then_StoresPerEnvironmentValues()
    {
        // Arrange
        var sut = CreateValid();
        var values = new Dictionary<string, string>
        {
            [DevEnvironment] = "dev-value",
            [ProdEnvironment] = "prod-value",
        };

        // Act
        var key = sut.AddStaticConfigurationKey(DefaultKey, DefaultLabel, values);

        // Assert
        sut.ConfigurationKeys.Should().ContainSingle();
        key.Key.Should().Be(DefaultKey);
        key.Label.Should().Be(DefaultLabel);
        key.IsStatic.Should().BeTrue();
        key.IsKeyVaultReference.Should().BeFalse();
        key.IsViaVariableGroup.Should().BeFalse();
        key.IsOutputReference.Should().BeFalse();
        key.EnvironmentValues.Should().HaveCount(2);
    }

    // ─── AddKeyVaultReferenceConfigurationKey ──────────────────────────────

    [Fact]
    public void Given_KeyVaultReference_When_AddKeyVaultReferenceConfigurationKey_Then_StoresReference()
    {
        // Arrange
        var sut = CreateValid();
        var keyVaultId = AzureResourceId.CreateUnique();

        // Act
        var key = sut.AddKeyVaultReferenceConfigurationKey(
            DefaultKey, DefaultLabel, keyVaultId, DefaultSecretName, SecretValueAssignment.ViaBicepparam);

        // Assert
        key.IsKeyVaultReference.Should().BeTrue();
        key.IsStatic.Should().BeFalse();
        key.KeyVaultResourceId.Should().Be(keyVaultId);
        key.SecretName.Should().Be(DefaultSecretName);
        key.SecretValueAssignment.Should().Be(SecretValueAssignment.ViaBicepparam);
    }

    // ─── AddViaVariableGroupConfigurationKey ───────────────────────────────

    [Fact]
    public void Given_VariableGroup_When_AddViaVariableGroupConfigurationKey_Then_StoresReference()
    {
        // Arrange
        var sut = CreateValid();
        var variableGroupId = ProjectPipelineVariableGroupId.CreateUnique();

        // Act
        var key = sut.AddViaVariableGroupConfigurationKey(
            DefaultKey, DefaultLabel, variableGroupId, DefaultPipelineVariableName);

        // Assert
        key.IsViaVariableGroup.Should().BeTrue();
        key.IsStatic.Should().BeFalse();
        key.VariableGroupId.Should().Be(variableGroupId);
        key.PipelineVariableName.Should().Be(DefaultPipelineVariableName);
    }

    // ─── AddViaVariableGroupKeyVaultReferenceConfigurationKey ──────────────

    [Fact]
    public void Given_VariableGroupAndKeyVault_When_Added_Then_StoresBothReferences()
    {
        // Arrange
        var sut = CreateValid();
        var variableGroupId = ProjectPipelineVariableGroupId.CreateUnique();
        var keyVaultId = AzureResourceId.CreateUnique();

        // Act
        var key = sut.AddViaVariableGroupKeyVaultReferenceConfigurationKey(
            DefaultKey, DefaultLabel, variableGroupId, DefaultPipelineVariableName,
            keyVaultId, DefaultSecretName, SecretValueAssignment.ViaBicepparam);

        // Assert
        key.IsViaVariableGroup.Should().BeTrue();
        key.IsKeyVaultReference.Should().BeTrue();
        key.IsStatic.Should().BeFalse();
        key.KeyVaultResourceId.Should().Be(keyVaultId);
        key.SecretName.Should().Be(DefaultSecretName);
        key.SecretValueAssignment.Should().Be(SecretValueAssignment.ViaBicepparam);
    }

    // ─── AddOutputReferenceConfigurationKey ────────────────────────────────

    [Fact]
    public void Given_OutputReference_When_AddOutputReferenceConfigurationKey_Then_StoresSourceReference()
    {
        // Arrange
        var sut = CreateValid();
        var sourceResourceId = AzureResourceId.CreateUnique();

        // Act
        var key = sut.AddOutputReferenceConfigurationKey(
            DefaultKey, DefaultLabel, sourceResourceId, DefaultSourceOutputName);

        // Assert
        key.IsOutputReference.Should().BeTrue();
        key.IsStatic.Should().BeFalse();
        key.IsKeyVaultReference.Should().BeFalse();
        key.SourceResourceId.Should().Be(sourceResourceId);
        key.SourceOutputName.Should().Be(DefaultSourceOutputName);
    }

    // ─── AddSensitiveOutputKeyVaultReferenceConfigurationKey ───────────────

    [Fact]
    public void Given_SensitiveOutputAndKeyVault_When_Added_Then_StoresBothReferencesAndDirectInKeyVaultAssignment()
    {
        // Arrange
        var sut = CreateValid();
        var sourceResourceId = AzureResourceId.CreateUnique();
        var keyVaultId = AzureResourceId.CreateUnique();

        // Act
        var key = sut.AddSensitiveOutputKeyVaultReferenceConfigurationKey(
            DefaultKey, DefaultLabel, sourceResourceId, DefaultSourceOutputName,
            keyVaultId, DefaultSecretName);

        // Assert
        key.IsOutputReference.Should().BeTrue();
        key.IsKeyVaultReference.Should().BeTrue();
        key.SourceResourceId.Should().Be(sourceResourceId);
        key.SourceOutputName.Should().Be(DefaultSourceOutputName);
        key.KeyVaultResourceId.Should().Be(keyVaultId);
        key.SecretName.Should().Be(DefaultSecretName);
        key.SecretValueAssignment.Should().Be(SecretValueAssignment.DirectInKeyVault);
    }

    // ─── RemoveConfigurationKey ────────────────────────────────────────────

    [Fact]
    public void Given_ExistingKey_When_RemoveConfigurationKey_Then_RemovesIt()
    {
        // Arrange
        var sut = CreateValid();
        var key = sut.AddStaticConfigurationKey(DefaultKey, DefaultLabel, new Dictionary<string, string>());

        // Act
        sut.RemoveConfigurationKey(key.Id);

        // Assert
        sut.ConfigurationKeys.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownKeyId_When_RemoveConfigurationKey_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValid();
        sut.AddStaticConfigurationKey(DefaultKey, DefaultLabel, new Dictionary<string, string>());

        // Act
        sut.RemoveConfigurationKey(InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects.AppConfigurationKeyId.CreateUnique());

        // Assert
        sut.ConfigurationKeys.Should().ContainSingle();
    }
}
