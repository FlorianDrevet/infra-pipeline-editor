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
                new BicepPropertyAssignment("properties", new BicepObjectExpression([
                    new BicepPropertyAssignment("template", new BicepObjectExpression([
                        new BicepPropertyAssignment("containers", new BicepArrayExpression([
                            new BicepObjectExpression([
                                new BicepPropertyAssignment("name", new BicepReference("name")),
                                new BicepPropertyAssignment("image", new BicepReference("containerRuntime.image")),
                            ]),
                        ])),
                    ])),
                ])),
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

    [Fact]
    public void Given_ContainerAppSpec_When_WithAppSettings_Then_WiresEnvVarsIntoContainerBody()
    {
        var spec = ContainerAppSpec();

        var result = spec.WithAppSettings(AzureResourceTypes.ArmTypes.ContainerApp);

        // Navigate: properties → template → containers[0] → env
        var props = result.Resource.Body.First(p => p.Key == "properties").Value as BicepObjectExpression;
        var template = props!.Properties.First(p => p.Key == "template").Value as BicepObjectExpression;
        var containers = template!.Properties.First(p => p.Key == "containers").Value as BicepArrayExpression;
        var firstContainer = containers!.Items[0] as BicepObjectExpression;

        firstContainer!.Properties.Should().Contain(p => p.Key == "env");
        var envValue = firstContainer.Properties.First(p => p.Key == "env").Value;
        envValue.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("envVars");
    }

    [Fact]
    public void Given_ContainerAppSpecWithExistingEnvVars_When_WithAppSettings_Then_DoesNotDuplicate()
    {
        var spec = ContainerAppSpec().WithAppSettings(AzureResourceTypes.ArmTypes.ContainerApp);

        var result = spec.WithAppSettings(AzureResourceTypes.ArmTypes.ContainerApp);

        result.Parameters.Count(p => p.Name == "envVars").Should().Be(1);
    }

    [Fact]
    public void Given_WebAppSpecWithExistingAppSettings_When_WithAppSettings_Then_ConcatenatesExistingAndInjectedSettings()
    {
        var spec = WebAppSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "app",
                ArmTypeWithApiVersion = "Microsoft.Web/sites@2023-12-01",
                Body =
                [
                    new BicepPropertyAssignment("name", new BicepReference("name")),
                    new BicepPropertyAssignment("location", new BicepReference("location")),
                    new BicepPropertyAssignment("properties", new BicepObjectExpression([
                        new BicepPropertyAssignment("siteConfig", new BicepObjectExpression([
                            new BicepPropertyAssignment("appSettings", new BicepReference("existingAppSettings")),
                        ])),
                    ])),
                ],
            },
        };

        var result = spec.WithAppSettings(AzureResourceTypes.ArmTypes.WebApp);

        var properties = (BicepObjectExpression)result.Resource.Body.First(property => property.Key == "properties").Value;
        var siteConfig = (BicepObjectExpression)properties.Properties.First(property => property.Key == "siteConfig").Value;
        var appSettings = siteConfig.Properties.First(property => property.Key == "appSettings").Value;

        appSettings.Should().BeOfType<BicepFunctionCall>();
        var concatCall = (BicepFunctionCall)appSettings;
        concatCall.Name.Should().Be("concat");
        concatCall.Arguments.Should().HaveCount(2);
        concatCall.Arguments[0].Should().BeEquivalentTo(new BicepReference("existingAppSettings"));
        concatCall.Arguments[1].Should().BeEquivalentTo(new BicepReference("appSettings"));
    }
}
