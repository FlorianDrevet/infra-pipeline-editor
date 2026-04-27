using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;

namespace InfraFlowSculptor.BicepGeneration.Tests.Ir;

public sealed class BicepModuleBuilderTests
{
    [Fact]
    public void Given_MinimalBuilder_When_Build_Then_ReturnsValidSpec()
    {
        var spec = new BicepModuleBuilder()
            .Module("keyVault", "key-vault", "Key Vault")
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Build();

        spec.ModuleName.Should().Be("keyVault");
        spec.ModuleFolderName.Should().Be("key-vault");
        spec.ResourceTypeName.Should().Be("Key Vault");
        spec.Resource.Symbol.Should().Be("kv");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.KeyVault/vaults@2023-07-01");
        spec.Resource.Body.Should().HaveCount(2);
    }

    [Fact]
    public void Given_BuilderWithParam_When_Build_Then_SpecContainsParam()
    {
        var spec = new BicepModuleBuilder()
            .Module("kv", "key-vault", "Key Vault")
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Param("name", BicepType.String, description: "The name")
            .Build();

        spec.Parameters.Should().ContainSingle()
            .Which.Name.Should().Be("name");
        spec.Parameters[0].Description.Should().Be("The name");
    }

    [Fact]
    public void Given_BuilderWithSecureParam_When_Build_Then_ParamIsSecure()
    {
        var spec = new BicepModuleBuilder()
            .Module("kv", "key-vault", "Key Vault")
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Param("secret", BicepType.String, secure: true)
            .Build();

        spec.Parameters.Should().ContainSingle()
            .Which.IsSecure.Should().BeTrue();
    }

    [Fact]
    public void Given_BuilderWithImport_When_Build_Then_SpecContainsImport()
    {
        var spec = new BicepModuleBuilder()
            .Module("kv", "key-vault", "Key Vault")
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Import("./types.bicep", "ManagedIdentityType")
            .Build();

        spec.Imports.Should().ContainSingle()
            .Which.Path.Should().Be("./types.bicep");
    }

    [Fact]
    public void Given_BuilderWithOutput_When_Build_Then_SpecContainsOutput()
    {
        var spec = new BicepModuleBuilder()
            .Module("kv", "key-vault", "Key Vault")
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Output("vaultId", BicepType.String, new BicepReference("kv.id"))
            .Build();

        spec.Outputs.Should().ContainSingle()
            .Which.Name.Should().Be("vaultId");
    }

    [Fact]
    public void Given_BuilderWithVar_When_Build_Then_SpecContainsVar()
    {
        var spec = new BicepModuleBuilder()
            .Module("kv", "key-vault", "Key Vault")
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Var("fullName", new BicepStringLiteral("test"))
            .Build();

        spec.Variables.Should().ContainSingle()
            .Which.Name.Should().Be("fullName");
    }

    [Fact]
    public void Given_BuilderWithNestedObject_When_Build_Then_BodyContainsNestedObject()
    {
        var spec = new BicepModuleBuilder()
            .Module("kv", "key-vault", "Key Vault")
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Property("properties", nested =>
                nested.Property("tenantId", new BicepReference("subscription().tenantId")))
            .Build();

        spec.Resource.Body.Should().ContainSingle()
            .Which.Value.Should().BeOfType<BicepObjectExpression>()
            .Which.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("tenantId");
    }

    [Fact]
    public void Given_BuilderWithExportedType_When_Build_Then_SpecContainsExportedType()
    {
        var spec = new BicepModuleBuilder()
            .Module("kv", "key-vault", "Key Vault")
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .ExportedType("MyType",
                new BicepRawExpression("'A' | 'B'"),
                description: "A union type")
            .Build();

        spec.ExportedTypes.Should().ContainSingle();
        spec.ExportedTypes[0].Name.Should().Be("MyType");
        spec.ExportedTypes[0].IsExported.Should().BeTrue();
    }

    [Fact]
    public void Given_BuilderWithoutResource_When_Build_Then_ThrowsInvalidOperation()
    {
        var builder = new BicepModuleBuilder().Module("kv", "key-vault", "Key Vault");

        var act = () => builder.Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Resource*");
    }

    [Fact]
    public void Given_BuilderWithCompanion_When_Build_Then_SpecContainsCompanion()
    {
        var companionSpec = new BicepModuleBuilder()
            .Module("kvSecret", "key-vault-secret", "Key Vault Secret")
            .Resource("secret", "Microsoft.KeyVault/vaults/secrets@2023-07-01")
            .Build();

        var spec = new BicepModuleBuilder()
            .Module("kv", "key-vault", "Key Vault")
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Companion("kvSecret", "key-vault-secret", companionSpec)
            .Build();

        spec.Companions.Should().ContainSingle()
            .Which.ModuleName.Should().Be("kvSecret");
    }
}
