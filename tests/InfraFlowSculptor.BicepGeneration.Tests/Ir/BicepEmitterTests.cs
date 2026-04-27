using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;

namespace InfraFlowSculptor.BicepGeneration.Tests.Ir;

public sealed class BicepEmitterTests
{
    private readonly BicepEmitter _sut = new();

    #region Helpers

    private static BicepModuleSpec MinimalSpec(
        Action<BicepModuleSpec>? customize = null)
    {
        var spec = new BicepModuleSpec
        {
            ModuleName = "keyVault",
            ModuleFolderName = "key-vault",
            ResourceTypeName = "Key Vault",
            Resource = new BicepResourceDeclaration
            {
                Symbol = "kv",
                ArmTypeWithApiVersion = "Microsoft.KeyVault/vaults@2023-07-01",
                Body =
                [
                    new BicepPropertyAssignment("name", new BicepReference("name")),
                    new BicepPropertyAssignment("location", new BicepReference("location")),
                ],
            },
        };
        customize?.Invoke(spec);
        return spec;
    }

    #endregion

    [Fact]
    public void Given_MinimalSpec_When_EmitModule_Then_ContainsResourceDeclaration()
    {
        var spec = MinimalSpec();

        var result = _sut.EmitModule(spec);

        result.Should().Contain("resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {");
        result.Should().Contain("  name: name");
        result.Should().Contain("  location: location");
        result.TrimEnd().Should().EndWith("}");
    }

    [Fact]
    public void Given_SpecWithParam_When_EmitModule_Then_EmitsParamWithDescription()
    {
        var spec = MinimalSpec() with
        {
            Parameters =
            [
                new BicepParam("name", BicepType.String, Description: "The resource name"),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("@description('The resource name')");
        result.Should().Contain("param name string");
    }

    [Fact]
    public void Given_SpecWithSecureParam_When_EmitModule_Then_EmitsSecureDecorator()
    {
        var spec = MinimalSpec() with
        {
            Parameters =
            [
                new BicepParam("secret", BicepType.String, IsSecure: true),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("@secure()");
        result.Should().Contain("param secret string");
    }

    [Fact]
    public void Given_SpecWithParamDefaultValue_When_EmitModule_Then_EmitsDefault()
    {
        var spec = MinimalSpec() with
        {
            Parameters =
            [
                new BicepParam("tags", BicepType.Object, DefaultValue: BicepObjectExpression.Empty),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("param tags object = {}");
    }

    [Fact]
    public void Given_SpecWithVariable_When_EmitModule_Then_EmitsVar()
    {
        var spec = MinimalSpec() with
        {
            Variables =
            [
                new BicepVar("fullName", new BicepInterpolation([
                    new BicepStringLiteral("kv-"),
                    new BicepReference("name"),
                ])),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("var fullName = 'kv-${name}'");
    }

    [Fact]
    public void Given_SpecWithOutput_When_EmitModule_Then_EmitsOutput()
    {
        var spec = MinimalSpec() with
        {
            Outputs =
            [
                new BicepOutput("vaultId", BicepType.String, new BicepReference("kv.id")),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("output vaultId string = kv.id");
    }

    [Fact]
    public void Given_SpecWithSecureOutput_When_EmitModule_Then_EmitsSecureDecorator()
    {
        var spec = MinimalSpec() with
        {
            Outputs =
            [
                new BicepOutput("connectionString", BicepType.String,
                    new BicepReference("kv.properties.vaultUri"), IsSecure: true),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("@secure()");
        result.Should().Contain("output connectionString string = kv.properties.vaultUri");
    }

    [Fact]
    public void Given_SpecWithImport_When_EmitModule_Then_EmitsImportAtTop()
    {
        var spec = MinimalSpec() with
        {
            Imports = [new BicepImport("./types.bicep", ["ManagedIdentityType"])],
        };

        var result = _sut.EmitModule(spec);

        result.Should().StartWith("import { ManagedIdentityType } from './types.bicep'");
    }

    [Fact]
    public void Given_SpecWithNestedObject_When_EmitModule_Then_EmitsNestedIndentation()
    {
        var spec = MinimalSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "kv",
                ArmTypeWithApiVersion = "Microsoft.KeyVault/vaults@2023-07-01",
                Body =
                [
                    new BicepPropertyAssignment("name", new BicepReference("name")),
                    new BicepPropertyAssignment("properties", new BicepObjectExpression([
                        new BicepPropertyAssignment("tenantId", new BicepReference("subscription().tenantId")),
                    ])),
                ],
            },
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("  properties: {");
        result.Should().Contain("    tenantId: subscription().tenantId");
        result.Should().Contain("  }");
    }

    [Fact]
    public void Given_SpecWithExportedTypes_When_EmitTypes_Then_EmitsExportedTypes()
    {
        var spec = MinimalSpec() with
        {
            ExportedTypes =
            [
                new BicepTypeDefinition("ManagedIdentityType",
                    new BicepRawExpression("'None' | 'SystemAssigned' | 'UserAssigned' | 'SystemAssigned, UserAssigned'"),
                    IsExported: true,
                    Description: "Managed identity type"),
            ],
        };

        var result = _sut.EmitTypes(spec);

        result.Should().Contain("@export()");
        result.Should().Contain("@description('Managed identity type')");
        result.Should().Contain("type ManagedIdentityType = 'None' | 'SystemAssigned' | 'UserAssigned' | 'SystemAssigned, UserAssigned'");
    }

    [Fact]
    public void Given_SpecWithNoExportedTypes_When_EmitTypes_Then_ReturnsEmpty()
    {
        var spec = MinimalSpec();

        var result = _sut.EmitTypes(spec);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Given_StringLiteral_When_Emit_Then_SingleQuotes()
    {
        var spec = MinimalSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "kv",
                ArmTypeWithApiVersion = "Microsoft.KeyVault/vaults@2023-07-01",
                Body =
                [
                    new BicepPropertyAssignment("name", new BicepStringLiteral("my-vault")),
                ],
            },
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("  name: 'my-vault'");
    }

    [Fact]
    public void Given_FunctionCall_When_Emit_Then_FormatsCorrectly()
    {
        var spec = MinimalSpec() with
        {
            Variables =
            [
                new BicepVar("result",
                    new BicepFunctionCall("concat", [
                        new BicepStringLiteral("a"),
                        new BicepStringLiteral("b"),
                    ])),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("var result = concat('a', 'b')");
    }

    [Fact]
    public void Given_ConditionalExpression_When_Emit_Then_FormatsTernary()
    {
        var spec = MinimalSpec() with
        {
            Variables =
            [
                new BicepVar("env",
                    new BicepConditionalExpression(
                        new BicepReference("isProd"),
                        new BicepStringLiteral("prod"),
                        new BicepStringLiteral("dev"))),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("var env = isProd ? 'prod' : 'dev'");
    }

    [Fact]
    public void Given_ArrayExpression_When_Emit_Then_FormatsInlineForSmall()
    {
        var spec = MinimalSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "kv",
                ArmTypeWithApiVersion = "Microsoft.KeyVault/vaults@2023-07-01",
                Body =
                [
                    new BicepPropertyAssignment("items",
                        new BicepArrayExpression([
                            new BicepStringLiteral("a"),
                            new BicepStringLiteral("b"),
                        ])),
                ],
            },
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("  items: [");
    }

    [Fact]
    public void Given_BoolLiteral_When_Emit_Then_LowerCase()
    {
        var spec = MinimalSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "kv",
                ArmTypeWithApiVersion = "Microsoft.KeyVault/vaults@2023-07-01",
                Body =
                [
                    new BicepPropertyAssignment("enabled", new BicepBoolLiteral(true)),
                ],
            },
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("  enabled: true");
    }

    [Fact]
    public void Given_IntLiteral_When_Emit_Then_PlainNumber()
    {
        var spec = MinimalSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "kv",
                ArmTypeWithApiVersion = "Microsoft.KeyVault/vaults@2023-07-01",
                Body =
                [
                    new BicepPropertyAssignment("count", new BicepIntLiteral(42)),
                ],
            },
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("  count: 42");
    }

    [Fact]
    public void Given_RawExpression_When_Emit_Then_OutputsVerbatim()
    {
        var spec = MinimalSpec() with
        {
            Variables =
            [
                new BicepVar("custom", new BicepRawExpression("resourceGroup().location")),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("var custom = resourceGroup().location");
    }

    [Fact]
    public void Given_MultipleParams_When_EmitModule_Then_BlankLineBetweenEach()
    {
        var spec = MinimalSpec() with
        {
            Parameters =
            [
                new BicepParam("name", BicepType.String, Description: "Name"),
                new BicepParam("location", BicepType.String, Description: "Location"),
            ],
        };

        var result = _sut.EmitModule(spec);

        // Each param block should be separated by a blank line
        var lines = result.Split('\n');
        var paramLines = lines
            .Select((l, i) => (Line: l.TrimEnd('\r'), Index: i))
            .Where(x => x.Line.StartsWith("param "))
            .ToList();

        paramLines.Should().HaveCount(2);
    }
}
