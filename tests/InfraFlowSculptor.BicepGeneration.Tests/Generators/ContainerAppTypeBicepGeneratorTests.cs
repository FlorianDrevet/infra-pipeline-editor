using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class ContainerAppTypeBicepGeneratorTests
{
    private readonly ContainerAppTypeBicepGenerator _sut = new();

    private const string PendingDnsValidationStatus = "Pending";
    private const string ValidatedDnsValidationStatus = "Validated";

    private static ResourceDefinition CreateNoAcrResource()
    {
        return new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-app",
            Type = AzureResourceTypes.ArmTypes.ContainerAppType,
            ResourceGroupName = "rg-test",
            ResourceAbbreviation = "ca",
            Properties = new Dictionary<string, string>(),
        };
    }

    private static ResourceDefinition CreateAcrMiResource()
    {
        return new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-app",
            Type = AzureResourceTypes.ArmTypes.ContainerAppType,
            ResourceGroupName = "rg-test",
            ResourceAbbreviation = "ca",
            Properties = new Dictionary<string, string>
            {
                ["containerRegistryId"] = "/subscriptions/.../registries/myacr",
                ["acrAuthMode"] = "ManagedIdentity",
            },
        };
    }

    private static ResourceDefinition CreateAcrDefaultAuthResource()
    {
        return new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-app",
            Type = AzureResourceTypes.ArmTypes.ContainerAppType,
            ResourceGroupName = "rg-test",
            ResourceAbbreviation = "ca",
            Properties = new Dictionary<string, string>
            {
                ["containerRegistryId"] = "/subscriptions/.../registries/myacr",
            },
        };
    }

    private static ResourceDefinition CreateAcrAdminResource()
    {
        return new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-app",
            Type = AzureResourceTypes.ArmTypes.ContainerAppType,
            ResourceGroupName = "rg-test",
            ResourceAbbreviation = "ca",
            Properties = new Dictionary<string, string>
            {
                ["containerRegistryId"] = "/subscriptions/.../registries/myacr",
                ["acrAuthMode"] = "AdminCredentials",
            },
        };
    }

    private static ResourceDefinition WithCustomDomains(
        ResourceDefinition resource,
        params CustomDomainDefinition[] customDomains)
    {
        resource.CustomDomains = customDomains;
        return resource;
    }

    private static CustomDomainDefinition CreatePendingCustomDomain(string domainName = "pending.contoso.com")
    {
        return CreateCustomDomain(domainName, PendingDnsValidationStatus);
    }

    private static CustomDomainDefinition CreateValidatedCustomDomain(string domainName = "validated.contoso.com")
    {
        return CreateCustomDomain(domainName, ValidatedDnsValidationStatus);
    }

    private static CustomDomainDefinition CreateCustomDomain(string domainName, string dnsValidationStatus)
    {
        return new CustomDomainDefinition
        {
            EnvironmentName = "dev",
            DomainName = domainName,
            CertificateMode = "ManagedCertificate",
            DnsValidationStatus = dnsValidationStatus,
        };
    }

    public static IEnumerable<object[]> LegacyResourcesWithoutValidatedCustomDomains()
    {
        yield return [WithCustomDomains(CreateNoAcrResource(), CreatePendingCustomDomain())];
        yield return [WithCustomDomains(CreateAcrMiResource(), CreatePendingCustomDomain())];
        yield return [WithCustomDomains(CreateAcrAdminResource(), CreatePendingCustomDomain())];
    }

    public static IEnumerable<object[]> LegacyResourcesWithValidatedCustomDomains()
    {
        yield return [WithCustomDomains(CreateNoAcrResource(), CreateValidatedCustomDomain())];
        yield return [WithCustomDomains(CreateAcrMiResource(), CreateValidatedCustomDomain())];
        yield return [WithCustomDomains(CreateAcrAdminResource(), CreateValidatedCustomDomain())];
    }

    // ── Interface contracts ──

    [Fact]
    public void Given_Generator_Then_ImplementsIResourceTypeBicepSpecGenerator()
    {
        _sut.Should().BeAssignableTo<IResourceTypeBicepSpecGenerator>();
    }

    [Fact]
    public void Given_Generator_Then_ResourceTypeIsCorrectArmType()
    {
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.ContainerAppType);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.ContainerApp);
    }

    // ── NoAcr variant: Module identity ──

    [Fact]
    public void Given_NoAcrResource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());

        spec.ModuleName.Should().Be("containerApp");
        spec.ModuleFolderName.Should().Be("ContainerApp");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.ContainerApp);
        spec.ModuleFileName.Should().BeNull("NoAcr variant uses default ModuleName");
    }

    [Fact]
    public void Given_NoAcrResource_When_GenerateSpec_Then_ImportsFourTypes()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("ContainerRuntimeConfig") &&
                i.Symbols.Contains("ScalingConfig") &&
                i.Symbols.Contains("IngressConfig") &&
                i.Symbols.Contains("HealthProbeConfig"));
    }

    // ── NoAcr variant: Params ──

    [Fact]
    public void Given_NoAcrResource_When_GenerateSpec_Then_HasEightParamsWhenNoValidatedCustomDomains()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        spec.Parameters.Should().HaveCount(8);
    }

    [Theory]
    [InlineData("location")]
    [InlineData("name")]
    [InlineData("containerAppEnvironmentId")]
    public void Given_NoAcrResource_When_GenerateSpec_Then_HasStringParam(string paramName)
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        spec.Parameters.Should().Contain(p => p.Name == paramName)
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_NoAcrResource_When_GenerateSpec_Then_ContainerImageDefaultsToAzureDocsHelloWorld()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());

        var parameter = spec.Parameters.Should().Contain(p => p.Name == "containerImage").Subject;

        parameter.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("mcr.microsoft.com/azuredocs/containerapps-helloworld:latest");
    }

    [Theory]
    [InlineData("containerRuntime", "ContainerRuntimeConfig")]
    [InlineData("scaling", "ScalingConfig")]
    [InlineData("ingress", "IngressConfig")]
    [InlineData("healthProbes", "HealthProbeConfig")]
    public void Given_NoAcrResource_When_GenerateSpec_Then_HasCustomTypeParam(string paramName, string typeName)
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == paramName).Subject;
        param.Type.Should().BeOfType<BicepCustomType>().Which.Name.Should().Be(typeName);
    }

    [Fact]
    public void Given_NoAcrResourceWithoutValidatedCustomDomains_When_GenerateSpec_Then_DoesNotDeclareCustomDomainsParam()
    {
        var resource = WithCustomDomains(CreateNoAcrResource(), CreatePendingCustomDomain());

        var spec = _sut.GenerateSpec(resource);

        spec.Parameters.Should().NotContain(p => p.Name == "customDomains");
    }

    // ── NoAcr variant: Variables ──

    [Fact]
    public void Given_NoAcrResource_When_GenerateSpec_Then_HasNoVariablesWhenNoValidatedCustomDomains()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        spec.Variables.Should().BeEmpty();
    }

    [Fact]
    public void Given_NoAcrResourceWithValidatedCustomDomains_When_GenerateSpec_Then_DeclaresCustomDomainSupport()
    {
        var resource = WithCustomDomains(
            CreateNoAcrResource(),
            CreatePendingCustomDomain(),
            CreateValidatedCustomDomain());

        var spec = _sut.GenerateSpec(resource);

        var parameter = spec.Parameters.Should().Contain(p => p.Name == "customDomains").Subject;
        parameter.Type.Should().Be(BicepType.Array);
        parameter.DefaultValue.Should().BeOfType<BicepArrayExpression>()
            .Which.Items.Should().BeEmpty();

        var variable = spec.Variables.Should().ContainSingle(v => v.Name == "customDomainBindings").Subject;
        variable.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Contain("for domain in customDomains");
    }

    // ── NoAcr variant: Resource ──

    [Fact]
    public void Given_NoAcrResource_When_GenerateSpec_Then_ResourceHasCorrectSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        spec.Resource.Symbol.Should().Be("containerApp");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.App/containerApps@2024-03-01");
    }

    [Fact]
    public void Given_NoAcrResource_When_GenerateSpec_Then_ConfigurationHasNoSecretsOrRegistries()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties").Subject;
        var propsObject = properties.Value.Should().BeOfType<BicepObjectExpression>().Subject;
        var configuration = propsObject.Properties.Should().Contain(p => p.Key == "configuration").Subject;
        var configObject = configuration.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        configObject.Properties.Should().NotContain(p => p.Key == "secrets");
        configObject.Properties.Should().NotContain(p => p.Key == "registries");
        configObject.Properties.Should().Contain(p => p.Key == "ingress");
    }

    [Fact]
    public void Given_NoAcrResourceWithoutValidatedCustomDomains_When_EmitModule_Then_OmitsCustomDomainsIngressMapping()
    {
        var resource = WithCustomDomains(CreateNoAcrResource(), CreatePendingCustomDomain());

        var emitted = new BicepEmitter().EmitModule(_sut.GenerateSpec(resource));

        emitted.Should().NotContain("param customDomains array");
        emitted.Should().NotContain("var customDomainBindings");
        emitted.Should().NotContain("customDomains:");
    }

    [Fact]
    public void Given_NoAcrResourceWithValidatedCustomDomains_When_EmitModule_Then_IncludesCustomDomainsIngressMapping()
    {
        var resource = WithCustomDomains(CreateNoAcrResource(), CreateValidatedCustomDomain());

        var emitted = new BicepEmitter().EmitModule(_sut.GenerateSpec(resource));

        emitted.Should().Contain("param customDomains array = []");
        emitted.Should().Contain("var customDomainBindings = [for domain in customDomains:");
        emitted.Should().Contain("customDomains: !empty(customDomains) ? customDomainBindings : null");
    }

    // ── NoAcr variant: Outputs ──

    [Fact]
    public void Given_NoAcrResource_When_GenerateSpec_Then_HasThreeOutputs()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        spec.Outputs.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("fqdn")]
    [InlineData("latestRevisionFqdn")]
    public void Given_NoAcrResource_When_GenerateSpec_Then_HasStringOutput(string outputName)
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        spec.Outputs.Should().Contain(o => o.Name == outputName)
            .Which.Type.Should().Be(BicepType.String);
    }

    // ── NoAcr variant: Exported types ──

    [Fact]
    public void Given_NoAcrResource_When_GenerateSpec_Then_HasSixExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        spec.ExportedTypes.Should().HaveCount(6);
    }

    [Theory]
    [InlineData("TransportMethod")]
    [InlineData("ContainerRuntimeConfig")]
    [InlineData("ScalingConfig")]
    [InlineData("IngressConfig")]
    [InlineData("ProbeConfig")]
    [InlineData("HealthProbeConfig")]
    public void Given_NoAcrResource_When_GenerateSpec_Then_HasExportedType(string typeName)
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        spec.ExportedTypes.Should().Contain(t => t.Name == typeName)
            .Which.IsExported.Should().BeTrue();
    }

    // ── ACR ManagedIdentity variant ──

    [Fact]
    public void Given_AcrMiResource_When_GenerateSpec_Then_ModuleFileNameIsContainerApp()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        spec.ModuleFileName.Should().Be("containerApp");
    }

    [Fact]
    public void Given_AcrMiResource_When_GenerateSpec_Then_HasNineParamsWhenNoValidatedCustomDomains()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        spec.Parameters.Should().HaveCount(10);
    }

    [Fact]
    public void Given_AcrMiResource_When_GenerateSpec_Then_HasAcrLoginServerParam()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        spec.Parameters.Should().Contain(p => p.Name == "acrLoginServer")
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_AcrMiResource_When_GenerateSpec_Then_NoAcrManagedIdentityClientIdParam()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        spec.Parameters.Should().NotContain(p => p.Name == "acrManagedIdentityClientId");
    }

    [Fact]
    public void Given_AcrMiResource_When_GenerateSpec_Then_NoSecureParams()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        spec.Parameters.Should().NotContain(p => p.IsSecure);
    }

    [Fact]
    public void Given_AcrMiResource_When_GenerateSpec_Then_ConfigurationHasRegistriesNoSecrets()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties").Subject;
        var propsObject = (BicepObjectExpression)properties.Value;
        var configuration = propsObject.Properties.Should().Contain(p => p.Key == "configuration").Subject;
        var configObject = (BicepObjectExpression)configuration.Value;

        configObject.Properties.Should().Contain(p => p.Key == "registries");
        configObject.Properties.Should().NotContain(p => p.Key == "secrets");
    }

    [Fact]
    public void Given_AcrMiResource_When_GenerateSpec_Then_RegistriesHasIdentityReferenceToUserAssignedIdentityId()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        var properties = (BicepObjectExpression)spec.Resource.Body.First(p => p.Key == "properties").Value;
        var configuration = (BicepObjectExpression)properties.Properties.First(p => p.Key == "configuration").Value;
        var registries = (BicepArrayExpression)configuration.Properties.First(p => p.Key == "registries").Value;
        var registry = (BicepObjectExpression)registries.Items[0];
        var identity = registry.Properties.Should().Contain(p => p.Key == "identity").Subject;

        identity.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("userAssignedIdentityId");
    }

    [Fact]
    public void Given_AcrMiResource_When_GenerateSpec_Then_HasNoVariablesWhenNoValidatedCustomDomains()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        spec.Variables.Should().BeEmpty();
    }

    [Fact]
    public void Given_AcrResourceWithoutAuthMode_When_GenerateSpec_Then_DefaultsToManagedIdentityVariant()
    {
        var spec = _sut.GenerateSpec(CreateAcrDefaultAuthResource());

        spec.ModuleFileName.Should().Be("containerApp");
        spec.Parameters.Should().NotContain(p => p.Name == "acrManagedIdentityClientId");
        spec.Parameters.Should().NotContain(p => p.Name == "acrPassword");
    }

    // ── ACR AdminCredentials variant ──

    [Fact]
    public void Given_AcrAdminResource_When_GenerateSpec_Then_ModuleFileNameIsContainerAppAcrAdminCredentials()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        spec.ModuleFileName.Should().Be("containerAppAcrAdminCredentials");
    }

    [Fact]
    public void Given_AcrAdminResource_When_GenerateSpec_Then_HasTenParamsWhenNoValidatedCustomDomains()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        spec.Parameters.Should().HaveCount(10);
    }

    [Fact]
    public void Given_AcrAdminResource_When_GenerateSpec_Then_HasSecureAcrPasswordParam()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "acrPassword").Subject;
        param.Type.Should().Be(BicepType.String);
        param.IsSecure.Should().BeTrue();
    }

    [Fact]
    public void Given_AcrAdminResource_When_GenerateSpec_Then_HasAcrLoginServerParam()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        spec.Parameters.Should().Contain(p => p.Name == "acrLoginServer");
    }

    [Fact]
    public void Given_AcrAdminResource_When_GenerateSpec_Then_HasTwoAcrVariablesWhenNoValidatedCustomDomains()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        spec.Variables.Should().HaveCount(2);
        spec.Variables.Should().Contain(v => v.Name == "acrUsername");
        spec.Variables.Should().Contain(v => v.Name == "acrPasswordSecretName");
    }

    [Fact]
    public void Given_AcrAdminResource_When_GenerateSpec_Then_AcrUsernameUsesSplit()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        var variable = spec.Variables.Should().Contain(v => v.Name == "acrUsername").Subject;
        variable.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Contain("split(acrLoginServer");
    }

    [Fact]
    public void Given_AcrAdminResource_When_GenerateSpec_Then_AcrPasswordSecretNameIsLiteral()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        var variable = spec.Variables.Should().Contain(v => v.Name == "acrPasswordSecretName").Subject;
        variable.Expression.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("acr-password");
    }

    [Fact]
    public void Given_AcrAdminResource_When_GenerateSpec_Then_ConfigurationHasSecretsAndRegistries()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        var properties = (BicepObjectExpression)spec.Resource.Body.First(p => p.Key == "properties").Value;
        var configuration = (BicepObjectExpression)properties.Properties.First(p => p.Key == "configuration").Value;

        configuration.Properties.Should().Contain(p => p.Key == "secrets");
        configuration.Properties.Should().Contain(p => p.Key == "registries");
    }

    [Fact]
    public void Given_AcrAdminResource_When_GenerateSpec_Then_RegistriesHasUsernameAndPasswordSecretRef()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        var properties = (BicepObjectExpression)spec.Resource.Body.First(p => p.Key == "properties").Value;
        var configuration = (BicepObjectExpression)properties.Properties.First(p => p.Key == "configuration").Value;
        var registries = (BicepArrayExpression)configuration.Properties.First(p => p.Key == "registries").Value;
        var registry = (BicepObjectExpression)registries.Items[0];

        registry.Properties.Should().Contain(p => p.Key == "server");
        registry.Properties.Should().Contain(p => p.Key == "username");
        registry.Properties.Should().Contain(p => p.Key == "passwordSecretRef");
    }

    // ── Legacy compat (Generate) ──

    [Fact]
    public void Given_NoAcrResource_When_Generate_Then_ModuleFileNameIsContainerApp()
    {
        var module = _sut.Generate(CreateNoAcrResource());
        module.ModuleFileName.Should().Be("containerApp.module.bicep");
    }

    [Fact]
    public void Given_NoAcrResource_When_Generate_Then_ModuleTemplateUsesAzureDocsHelloWorldDefault()
    {
        var module = _sut.Generate(CreateNoAcrResource());

        module.ModuleBicepContent.Should().Contain("param containerImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'");
    }

    [Fact]
    public void Given_AcrMiResource_When_Generate_Then_ModuleFileNameMatchesVariant()
    {
        var module = _sut.Generate(CreateAcrMiResource());
        module.ModuleFileName.Should().Be("containerApp.module.bicep");
    }

    [Fact]
    public void Given_AcrAdminResource_When_Generate_Then_ModuleFileNameMatchesVariant()
    {
        var module = _sut.Generate(CreateAcrAdminResource());
        module.ModuleFileName.Should().Be("containerAppAcrAdminCredentials.module.bicep");
    }

    [Fact]
    public void Given_AcrAdminResource_When_Generate_Then_AcrPasswordIsInSecureParameters()
    {
        var module = _sut.Generate(CreateAcrAdminResource());
        module.SecureParameters.Should().Contain("acrPassword");
    }

    [Fact]
    public void Given_NoAcrResource_When_Generate_Then_NoSecureParameters()
    {
        var module = _sut.Generate(CreateNoAcrResource());
        module.SecureParameters.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(LegacyResourcesWithoutValidatedCustomDomains))]
    public void Given_ResourceWithoutValidatedCustomDomains_When_Generate_Then_LegacyModuleOmitsCustomDomainSupport(
        ResourceDefinition resource)
    {
        var module = _sut.Generate(resource);

        module.Parameters.Should().NotContainKey("customDomains");
        module.ModuleBicepContent.Should().NotContain("param customDomains array");
        module.ModuleBicepContent.Should().NotContain("var customDomainBindings");
        module.ModuleBicepContent.Should().NotContain("customDomains:");
    }

    [Theory]
    [MemberData(nameof(LegacyResourcesWithValidatedCustomDomains))]
    public void Given_ResourceWithValidatedCustomDomains_When_Generate_Then_LegacyModuleIncludesCustomDomainSupport(
        ResourceDefinition resource)
    {
        var module = _sut.Generate(resource);

        module.Parameters.Should().ContainKey("customDomains");
        module.ModuleBicepContent.Should().Contain("param customDomains array = []");
        module.ModuleBicepContent.Should().Contain("var customDomainBindings = [for domain in customDomains:");
        module.ModuleBicepContent.Should().Contain("customDomains: !empty(customDomains) ? customDomainBindings : null");
    }

    [Fact]
    public void Given_NoAcrResource_When_Generate_Then_GroupedParameterTypeOverridesStayAligned()
    {
        var module = _sut.Generate(CreateNoAcrResource());

        module.ParameterTypeOverrides.Should().Contain(
            new KeyValuePair<string, string>("containerRuntime", "ContainerRuntimeConfig"),
            new KeyValuePair<string, string>("scaling", "ScalingConfig"),
            new KeyValuePair<string, string>("ingress", "IngressConfig"),
            new KeyValuePair<string, string>("healthProbes", "HealthProbeConfig"));
    }

    // ── Emission ──

    [Fact]
    public void Given_NoAcrResource_When_EmitModule_Then_ContainsResourceDeclaration()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        var emitted = new BicepEmitter().EmitModule(spec);

        emitted.Should().Contain("resource containerApp 'Microsoft.App/containerApps@2024-03-01'");
        emitted.Should().Contain("output id string = containerApp.id");
    }

    [Fact]
    public void Given_AcrMiResource_When_EmitModule_Then_ContainsRegistriesNoSecrets()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        var emitted = new BicepEmitter().EmitModule(spec);

        emitted.Should().Contain("registries");
        emitted.Should().NotContain("secrets:");
    }

    [Fact]
    public void Given_AcrMiResource_When_EmitModule_Then_IndentsHealthProbeUnion()
    {
        var spec = _sut.GenerateSpec(CreateAcrMiResource());
        var emitted = new BicepEmitter().EmitModule(spec);

        emitted.Should().Contain(
            "          probes: union(\n" +
            "            !empty(healthProbes.readiness.path)");
    }

    [Fact]
    public void Given_AcrAdminResource_When_EmitModule_Then_ContainsSecretsAndSecureAcrPassword()
    {
        var spec = _sut.GenerateSpec(CreateAcrAdminResource());
        var emitted = new BicepEmitter().EmitModule(spec);

        emitted.Should().Contain("@secure()");
        emitted.Should().Contain("param acrPassword string");
        emitted.Should().Contain("secrets");
    }

    [Fact]
    public void Given_NoAcrResource_When_EmitTypes_Then_ContainsAllSixTypes()
    {
        var spec = _sut.GenerateSpec(CreateNoAcrResource());
        var emitted = new BicepEmitter().EmitTypes(spec);

        emitted.Should().Contain("type TransportMethod");
        emitted.Should().Contain("type ContainerRuntimeConfig");
        emitted.Should().Contain("type ScalingConfig");
        emitted.Should().Contain("type IngressConfig");
        emitted.Should().Contain("type ProbeConfig");
        emitted.Should().Contain("type HealthProbeConfig");
    }
}
