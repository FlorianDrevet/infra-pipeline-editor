using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;

namespace InfraFlowSculptor.BicepGeneration.Tests.Ir;

public sealed class BicepEmitterTests
{
    private readonly BicepEmitter _sut = new();

    #region Helpers

    private static BicepModuleSpec MinimalSpec() => new()
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

    #region ExistingResource emission

    [Fact]
    public void Given_SpecWithExistingResource_When_EmitModule_Then_EmitsExistingBlock()
    {
        var spec = MinimalSpec() with
        {
            ExistingResources =
            [
                new BicepExistingResource("sqlServer", "Microsoft.Sql/servers@2023-05-01-preview", "sqlServerName"),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' existing = {");
        result.Should().Contain("  name: sqlServerName");
    }

    #endregion

    #region Resource with Condition

    [Fact]
    public void Given_ResourceWithCondition_When_EmitModule_Then_EmitsIfGuard()
    {
        var spec = MinimalSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "kv",
                ArmTypeWithApiVersion = "Microsoft.KeyVault/vaults@2023-07-01",
                Condition = new BicepReference("deployVault"),
                Body =
                [
                    new BicepPropertyAssignment("name", new BicepReference("name")),
                ],
            },
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = if (deployVault) {");
    }

    #endregion

    #region Resource with ForLoop

    [Fact]
    public void Given_ResourceWithForLoop_When_EmitModule_Then_EmitsForSyntax()
    {
        var spec = MinimalSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "app",
                ArmTypeWithApiVersion = "Microsoft.Web/sites@2023-12-01",
                ForLoop = new BicepForLoop("item", new BicepReference("items")),
                Body =
                [
                    new BicepPropertyAssignment("name", new BicepReference("item.name")),
                ],
            },
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("resource app 'Microsoft.Web/sites@2023-12-01' = [for item in items: {");
        result.Should().Contain("  name: item.name");
        result.TrimEnd().Should().EndWith("}]");
    }

    #endregion

    #region Resource with ParentSymbol and Scope

    [Fact]
    public void Given_ResourceWithParentSymbol_When_EmitModule_Then_EmitsParentLine()
    {
        var spec = MinimalSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "db",
                ArmTypeWithApiVersion = "Microsoft.Sql/servers/databases@2023-05-01-preview",
                ParentSymbol = "sqlServer",
                Body =
                [
                    new BicepPropertyAssignment("name", new BicepReference("databaseName")),
                ],
            },
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("  parent: sqlServer");
        result.Should().Contain("  name: databaseName");
    }

    [Fact]
    public void Given_ResourceWithScope_When_EmitModule_Then_EmitsScopeLine()
    {
        var spec = MinimalSpec() with
        {
            Resource = new BicepResourceDeclaration
            {
                Symbol = "diag",
                ArmTypeWithApiVersion = "Microsoft.Insights/diagnosticSettings@2021-05-01-preview",
                Scope = "kv",
                Body =
                [
                    new BicepPropertyAssignment("name", new BicepStringLiteral("diag")),
                ],
            },
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("  scope: kv");
    }

    #endregion

    #region AdditionalResources

    [Fact]
    public void Given_SpecWithAdditionalResource_When_EmitModule_Then_EmitsAfterPrimaryResource()
    {
        var spec = MinimalSpec() with
        {
            AdditionalResources =
            [
                new BicepResourceDeclaration
                {
                    Symbol = "roleAssignment",
                    ArmTypeWithApiVersion = "Microsoft.Authorization/roleAssignments@2022-04-01",
                    Scope = "kv",
                    Condition = new BicepReference("assignRole"),
                    Body =
                    [
                        new BicepPropertyAssignment("name", new BicepRawExpression("guid(kv.id)")),
                    ],
                },
            ],
        };

        var result = _sut.EmitModule(spec);

        // Primary resource should appear first
        result.Should().Contain("resource kv 'Microsoft.KeyVault/vaults@2023-07-01'");
        // Additional resource after
        result.Should().Contain("resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (assignRole) {");
        result.Should().Contain("  scope: kv");
    }

    #endregion

    #region Output with description

    [Fact]
    public void Given_OutputWithDescription_When_EmitModule_Then_EmitsDescriptionDecorator()
    {
        var spec = MinimalSpec() with
        {
            Outputs =
            [
                new BicepOutput("vaultUri", BicepType.String,
                    new BicepRawExpression("kv.properties.vaultUri"),
                    Description: "The URI of the vault"),
            ],
        };

        var result = _sut.EmitModule(spec);

        result.Should().Contain("@description('The URI of the vault')");
        result.Should().Contain("output vaultUri string = kv.properties.vaultUri");
    }

    #endregion

    #region EmitTypes with multiple types

    [Fact]
    public void Given_SpecWithMultipleExportedTypes_When_EmitTypes_Then_EmitsAllTypes()
    {
        var spec = MinimalSpec() with
        {
            ExportedTypes =
            [
                new BicepTypeDefinition("SkuName",
                    new BicepRawExpression("'Free' | 'Standard' | 'Premium'"),
                    IsExported: true, Description: "SKU name"),
                new BicepTypeDefinition("Tier",
                    new BicepRawExpression("'Basic' | 'Standard'"),
                    IsExported: true),
            ],
        };

        var result = _sut.EmitTypes(spec);

        result.Should().Contain("type SkuName = 'Free' | 'Standard' | 'Premium'");
        result.Should().Contain("type Tier = 'Basic' | 'Standard'");
        result.Should().Contain("@description('SKU name')");
    }

    #endregion
}
