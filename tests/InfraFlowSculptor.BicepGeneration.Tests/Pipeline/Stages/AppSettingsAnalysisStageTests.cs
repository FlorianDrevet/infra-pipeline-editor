using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class AppSettingsAnalysisStageTests
{
    private readonly AppSettingsAnalysisStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns200()
    {
        _sut.Order.Should().Be(200);
    }

    [Fact]
    public void Given_NoAppSettings_When_Execute_Then_AppSettingsResultIsEmpty()
    {
        // Arrange
        var context = CreateContext([], []);

        // Act
        _sut.Execute(context);

        // Assert
        context.AppSettings.OutputsBySourceResource.Should().BeEmpty();
        context.AppSettings.ComputeArmTypesWithAppSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_OutputReferenceAppSetting_When_Execute_Then_OutputsBySourceResourcePopulated()
    {
        // Arrange
        var resources = new[]
        {
            CreateResource("my-kv", AzureResourceTypes.ArmTypes.KeyVault),
            CreateResource("my-webapp", AzureResourceTypes.ArmTypes.WebApp),
        };
        var appSettings = new[]
        {
            new AppSettingDefinition
            {
                Name = "KeyVaultUri",
                TargetResourceName = "my-webapp",
                SourceResourceName = "my-kv",
                SourceOutputName = "vaultUri",
                SourceOutputBicepExpression = "kv.properties.vaultUri",
                IsOutputReference = true,
            },
        };
        var context = CreateContext(resources, appSettings);

        // Act
        _sut.Execute(context);

        // Assert
        context.AppSettings.OutputsBySourceResource.Should().ContainKey("my-kv");
        var outputs = context.AppSettings.OutputsBySourceResource["my-kv"];
        outputs.Should().ContainSingle(o => o.OutputName == "vaultUri");
    }

    [Fact]
    public void Given_SensitiveOutputExportedToKeyVault_When_Execute_Then_IncludedInOutputs()
    {
        // Arrange
        var resources = new[]
        {
            CreateResource("my-sql", AzureResourceTypes.ArmTypes.SqlServer),
            CreateResource("my-webapp", AzureResourceTypes.ArmTypes.WebApp),
        };
        var appSettings = new[]
        {
            new AppSettingDefinition
            {
                Name = "SqlConnectionString",
                TargetResourceName = "my-webapp",
                SourceResourceName = "my-sql",
                SourceOutputName = "connectionString",
                SourceOutputBicepExpression = "'${sqlServer.properties.fqdn}'",
                IsSensitiveOutputExportedToKeyVault = true,
            },
        };
        var context = CreateContext(resources, appSettings);

        // Act
        _sut.Execute(context);

        // Assert
        context.AppSettings.OutputsBySourceResource.Should().ContainKey("my-sql");
        context.AppSettings.OutputsBySourceResource["my-sql"]
            .Should().ContainSingle(o => o.IsSecure);
    }

    [Fact]
    public void Given_StaticAppSetting_When_Execute_Then_NotIncludedInOutputs()
    {
        // Arrange
        var resources = new[]
        {
            CreateResource("my-webapp", AzureResourceTypes.ArmTypes.WebApp),
        };
        var appSettings = new[]
        {
            new AppSettingDefinition
            {
                Name = "MyStaticSetting",
                TargetResourceName = "my-webapp",
                StaticValue = "hello",
                IsOutputReference = false,
            },
        };
        var context = CreateContext(resources, appSettings);

        // Act
        _sut.Execute(context);

        // Assert
        context.AppSettings.OutputsBySourceResource.Should().BeEmpty();
    }

    [Fact]
    public void Given_ComputeResourceWithAppSettings_When_Execute_Then_ComputeArmTypesPopulated()
    {
        // Arrange
        var resources = new[]
        {
            CreateResource("my-kv", AzureResourceTypes.ArmTypes.KeyVault),
            CreateResource("my-webapp", AzureResourceTypes.ArmTypes.WebApp),
        };
        var appSettings = new[]
        {
            new AppSettingDefinition
            {
                Name = "Setting",
                TargetResourceName = "my-webapp",
                SourceResourceName = "my-kv",
                SourceOutputName = "vaultUri",
                SourceOutputBicepExpression = "kv.properties.vaultUri",
                IsOutputReference = true,
            },
        };
        var context = CreateContext(resources, appSettings);

        // Act
        _sut.Execute(context);

        // Assert
        context.AppSettings.ComputeArmTypesWithAppSettings
            .Should().Contain(AzureResourceTypes.ArmTypes.WebApp);
    }

    [Fact]
    public void Given_NonComputeResourceTarget_When_Execute_Then_ComputeArmTypesEmpty()
    {
        // Arrange — target is a Key Vault (not a compute type)
        var resources = new[]
        {
            CreateResource("my-kv", AzureResourceTypes.ArmTypes.KeyVault),
        };
        var appSettings = new[]
        {
            new AppSettingDefinition
            {
                Name = "Setting",
                TargetResourceName = "my-kv",
                StaticValue = "val",
            },
        };
        var context = CreateContext(resources, appSettings);

        // Act
        _sut.Execute(context);

        // Assert
        context.AppSettings.ComputeArmTypesWithAppSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_DuplicateOutputNames_When_Execute_Then_Deduplicated()
    {
        // Arrange
        var resources = new[]
        {
            CreateResource("my-kv", AzureResourceTypes.ArmTypes.KeyVault),
            CreateResource("my-webapp", AzureResourceTypes.ArmTypes.WebApp),
        };
        var appSettings = new[]
        {
            new AppSettingDefinition
            {
                Name = "Setting1",
                TargetResourceName = "my-webapp",
                SourceResourceName = "my-kv",
                SourceOutputName = "vaultUri",
                SourceOutputBicepExpression = "kv.properties.vaultUri",
                IsOutputReference = true,
            },
            new AppSettingDefinition
            {
                Name = "Setting2",
                TargetResourceName = "my-webapp",
                SourceResourceName = "my-kv",
                SourceOutputName = "vaultUri",
                SourceOutputBicepExpression = "kv.properties.vaultUri",
                IsOutputReference = true,
            },
        };
        var context = CreateContext(resources, appSettings);

        // Act
        _sut.Execute(context);

        // Assert — same output name from same source should appear once
        context.AppSettings.OutputsBySourceResource["my-kv"]
            .Should().ContainSingle(o => o.OutputName == "vaultUri");
    }

    // ── Helpers ──

    private static BicepGenerationContext CreateContext(
        ResourceDefinition[] resources,
        AppSettingDefinition[] appSettings) =>
        new()
        {
            Request = new GenerationRequest
            {
                Resources = resources,
                AppSettings = appSettings,
            },
        };

    private static ResourceDefinition CreateResource(string name, string type) =>
        new()
        {
            ResourceId = Guid.NewGuid(),
            Name = name,
            Type = type,
        };
}
