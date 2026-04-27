using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Transformations;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Tests.Ir;

public sealed class AppSettingsTransformerTests
{
    private static BicepModuleSpec WebAppSpec() => new()
    {
        ModuleName = "webApp",
        ModuleFolderName = "web-app",
        ResourceTypeName = "Web App",
        Resource = new BicepResourceDeclaration
        {
            Symbol = "app",
            ArmTypeWithApiVersion = "Microsoft.Web/sites@2023-12-01",
            Body =
            [
                new BicepPropertyAssignment("name", new BicepReference("name")),
                new BicepPropertyAssignment("location", new BicepReference("location")),
                new BicepPropertyAssignment("properties", new BicepObjectExpression([
                    new BicepPropertyAssignment("siteConfig", BicepObjectExpression.Empty),
                ])),
            ],
        },
    };

    private static BicepModuleSpec ContainerAppSpec() => new()
    {
        ModuleName = "containerApp",
        ModuleFolderName = "container-app",
        ResourceTypeName = "Container App",
        Resource = new BicepResourceDeclaration
        {
            Symbol = "app",
            ArmTypeWithApiVersion = "Microsoft.App/containerApps@2024-03-01",
            Body =
            [
                new BicepPropertyAssignment("name", new BicepReference("name")),
                new BicepPropertyAssignment("location", new BicepReference("location")),
                new BicepPropertyAssignment("properties", BicepObjectExpression.Empty),
            ],
        },
    };

    [Fact]
    public void Given_WebAppSpec_When_WithAppSettings_Then_AddsAppSettingsParam()
    {
        var spec = WebAppSpec();

        var result = spec.WithAppSettings(AzureResourceTypes.ArmTypes.WebApp);

        result.Parameters.Should().Contain(p => p.Name == "appSettings");
    }

    [Fact]
    public void Given_ContainerAppSpec_When_WithAppSettings_Then_AddsEnvVarsParam()
    {
        var spec = ContainerAppSpec();

        var result = spec.WithAppSettings(AzureResourceTypes.ArmTypes.ContainerApp);

        result.Parameters.Should().Contain(p => p.Name == "envVars");
    }

    [Fact]
    public void Given_WebAppSpec_When_WithAppSettings_Then_AppSettingsParamIsArrayType()
    {
        var spec = WebAppSpec();

        var result = spec.WithAppSettings(AzureResourceTypes.ArmTypes.WebApp);

        var param = result.Parameters.First(p => p.Name == "appSettings");
        param.Type.Should().Be(BicepType.Array);
    }

    [Fact]
    public void Given_SpecWithExistingAppSettingsParam_When_WithAppSettings_Then_DoesNotDuplicate()
    {
        var spec = WebAppSpec().WithAppSettings(AzureResourceTypes.ArmTypes.WebApp);

        var result = spec.WithAppSettings(AzureResourceTypes.ArmTypes.WebApp);

        result.Parameters.Count(p => p.Name == "appSettings").Should().Be(1);
    }
}
