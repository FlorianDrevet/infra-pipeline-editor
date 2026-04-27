using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class WebAppTypeBicepGeneratorTests
{
    private readonly WebAppTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateCodeResource(
        Dictionary<string, string>? properties = null)
    {
        var props = properties ?? new Dictionary<string, string>();
        props.TryAdd("deploymentMode", "Code");
        props.TryAdd("runtimeStack", "DOTNETCORE");
        props.TryAdd("runtimeVersion", "8.0");
        props.TryAdd("alwaysOn", "true");
        props.TryAdd("httpsOnly", "true");
        return new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-webapp",
            Type = AzureResourceTypes.ArmTypes.WebApp,
            ResourceGroupName = "rg-test",
            ResourceAbbreviation = "app",
            Properties = props,
        };
    }

    private static ResourceDefinition CreateContainerMiResource()
    {
        return CreateCodeResource(new Dictionary<string, string>
        {
            ["deploymentMode"] = "Container",
            ["acrAuthMode"] = "ManagedIdentity",
            ["runtimeStack"] = "DOTNETCORE",
            ["runtimeVersion"] = "8.0",
            ["alwaysOn"] = "true",
            ["httpsOnly"] = "true",
            ["dockerImageName"] = "myapp/api",
        });
    }

    private static ResourceDefinition CreateContainerAdminResource()
    {
        return CreateCodeResource(new Dictionary<string, string>
        {
            ["deploymentMode"] = "Container",
            ["acrAuthMode"] = "AdminCredentials",
            ["runtimeStack"] = "DOTNETCORE",
            ["runtimeVersion"] = "8.0",
            ["alwaysOn"] = "true",
            ["httpsOnly"] = "true",
            ["dockerImageName"] = "myapp/api",
        });
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.WebApp);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.WebApp);
    }

    // ── Spec structure (Code variant) ──

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());

        spec.ModuleName.Should().Be("webApp");
        spec.ModuleFolderName.Should().Be("WebApp");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.WebApp);
        spec.ModuleFileName.Should().BeNull("Code variant uses default ModuleName");
    }

    // ── Imports ──

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_ImportsRuntimeStackFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("RuntimeStack"));
    }

    // ── Code variant: Params ──

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasNineParams()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Parameters.Should().HaveCount(9);
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasLocationParam()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Parameters.Should().Contain(p => p.Name == "location")
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasNameParam()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Parameters.Should().Contain(p => p.Name == "name")
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasAppServicePlanIdParam()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Parameters.Should().Contain(p => p.Name == "appServicePlanId")
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasRuntimeStackParam()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "runtimeStack").Which;

        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("RuntimeStack");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("DOTNETCORE");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasRuntimeVersionParam()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Parameters.Should().Contain(p => p.Name == "runtimeVersion")
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasAlwaysOnParam()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Parameters.Should().Contain(p => p.Name == "alwaysOn")
            .Which.Type.Should().Be(BicepType.Bool);
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasHttpsOnlyParam()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Parameters.Should().Contain(p => p.Name == "httpsOnly")
            .Which.Type.Should().Be(BicepType.Bool);
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasDeploymentModeParam()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "deploymentMode").Which;

        param.Type.Should().Be(BicepType.String);
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Code");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasCustomDomainsParam()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "customDomains").Which;

        param.Type.Should().Be(BicepType.Array);
        param.DefaultValue.Should().BeOfType<BicepArrayExpression>()
            .Which.Items.Should().BeEmpty();
    }

    // ── Code variant: Variables ──

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasLinuxFxVersionVar()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());

        spec.Variables.Should().ContainSingle()
            .Which.Name.Should().Be("linuxFxVersion");
    }

    // ── Code variant: Primary resource ──

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_ResourceHasCorrectSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());

        spec.Resource.Symbol.Should().Be("webApp");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Web/sites@2023-12-01");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_ResourceHasNameAndLocation()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());

        spec.Resource.Body.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("name");

        spec.Resource.Body.Should().Contain(p => p.Key == "location")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("location");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_ResourceHasNoKind()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Resource.Body.Should().NotContain(p => p.Key == "kind");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_PropertiesHasServerFarmId()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        properties.Properties.Should().Contain(p => p.Key == "serverFarmId")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("appServicePlanId");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_SiteConfigHasLinuxFxVersionRef()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var siteConfig = properties.Properties.Should().Contain(p => p.Key == "siteConfig")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        siteConfig.Properties.Should().Contain(p => p.Key == "linuxFxVersion")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("linuxFxVersion");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_SiteConfigHasFtpsAndTls()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var siteConfig = properties.Properties.Should().Contain(p => p.Key == "siteConfig")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        siteConfig.Properties.Should().Contain(p => p.Key == "ftpsState")
            .Which.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Disabled");

        siteConfig.Properties.Should().Contain(p => p.Key == "minTlsVersion")
            .Which.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("1.2");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_CodeVariantHasNoAcrProps()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var siteConfig = properties.Properties.Should().Contain(p => p.Key == "siteConfig")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        siteConfig.Properties.Should().NotContain(p => p.Key == "acrUseManagedIdentityCreds");
        siteConfig.Properties.Should().NotContain(p => p.Key == "acrUserManagedIdentityID");
        siteConfig.Properties.Should().NotContain(p => p.Key == "appSettings");
    }

    // ── Code variant: hostNameBindings (for-loop additional resource) ──

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasHostNameBindingsAdditionalResource()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.AdditionalResources.Should().ContainSingle();
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HostNameBindingsHasForLoop()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var hostBindings = spec.AdditionalResources[0];

        hostBindings.Symbol.Should().Be("hostNameBindings");
        hostBindings.ArmTypeWithApiVersion.Should().Be("Microsoft.Web/sites/hostNameBindings@2023-12-01");

        hostBindings.ForLoop.Should().NotBeNull();
        hostBindings.ForLoop!.IteratorName.Should().Be("domain");
        hostBindings.ForLoop.Collection.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("customDomains");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HostNameBindingsHasParent()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var hostBindings = spec.AdditionalResources[0];

        hostBindings.ParentSymbol.Should().Be("webApp");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HostNameBindingsHasBodyProperties()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var hostBindings = spec.AdditionalResources[0];

        hostBindings.Body.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("domain.domainName");

        var props = hostBindings.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        props.Properties.Should().Contain(p => p.Key == "siteName")
            .Which.Value.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("webApp.name");

        props.Properties.Should().Contain(p => p.Key == "hostNameType")
            .Which.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Verified");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HostNameBindingsSslStateIsConditional()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var hostBindings = spec.AdditionalResources[0];

        var props = hostBindings.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var sslState = props.Properties.Should().Contain(p => p.Key == "sslState")
            .Which.Value.Should().BeOfType<BicepConditionalExpression>().Subject;

        sslState.Condition.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("domain.bindingType == 'SniEnabled'");
        sslState.Consequent.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("SniEnabled");
        sslState.Alternate.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Disabled");
    }

    // ── Code variant: Outputs ──

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasFourOutputs()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Outputs.Should().HaveCount(4);
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasIdOutput()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var output = spec.Outputs.Should().Contain(o => o.Name == "id").Which;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("webApp.id");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasDefaultHostNameOutput()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Outputs.Should().Contain(o => o.Name == "defaultHostName")
            .Which.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("webApp.properties.defaultHostName");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasPrincipalIdOutput()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Outputs.Should().Contain(o => o.Name == "principalId")
            .Which.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("webApp.identity.principalId");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasCustomDomainVerificationIdOutput()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.Outputs.Should().Contain(o => o.Name == "customDomainVerificationId")
            .Which.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("webApp.properties.customDomainVerificationId");
    }

    // ── Code variant: Exported types ──

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasTwoExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.ExportedTypes.Should().HaveCount(2);
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasRuntimeStackExportedType()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.ExportedTypes.Should().Contain(t => t.Name == "RuntimeStack")
            .Which.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'DOTNETCORE' | 'NODE' | 'PYTHON' | 'JAVA' | 'PHP'");
    }

    [Fact]
    public void Given_CodeResource_When_GenerateSpec_Then_HasDeploymentModeExportedType()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        spec.ExportedTypes.Should().Contain(t => t.Name == "DeploymentMode")
            .Which.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'Code' | 'Container'");
    }

    // ── Container MI variant: Module identity ──

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_ModuleFileNameIsContainerMi()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());

        spec.ModuleName.Should().Be("webApp");
        spec.ModuleFileName.Should().Be("webAppContainerManagedIdentity");
    }

    // ── Container MI variant: Additional params ──

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_HasFourteenParams()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        spec.Parameters.Should().HaveCount(14);
    }

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_HasDockerImageNameParam()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        spec.Parameters.Should().Contain(p => p.Name == "dockerImageName")
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_HasDockerImageTagParam()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "dockerImageTag").Which;
        param.Type.Should().Be(BicepType.String);
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("latest");
    }

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_HasAcrLoginServerParam()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        spec.Parameters.Should().Contain(p => p.Name == "acrLoginServer")
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_HasAcrManagedIdentityParams()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());

        var acrUseMi = spec.Parameters.Should().Contain(p => p.Name == "acrUseManagedIdentityCreds").Which;
        acrUseMi.Type.Should().Be(BicepType.Bool);
        acrUseMi.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeTrue();

        var acrUaiId = spec.Parameters.Should().Contain(p => p.Name == "acrUserManagedIdentityId").Which;
        acrUaiId.Type.Should().Be(BicepType.String);
        acrUaiId.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().BeEmpty();
    }

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_DeploymentModeDefaultIsContainer()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        spec.Parameters.Should().Contain(p => p.Name == "deploymentMode")
            .Which.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Container");
    }

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_HasNoSecureParams()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        spec.Parameters.Should().NotContain(p => p.IsSecure);
    }

    // ── Container MI variant: Variables ──

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_HasDockerImageVar()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        spec.Variables.Should().ContainSingle()
            .Which.Name.Should().Be("dockerImage");
    }

    // ── Container MI variant: Resource ──

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_ResourceHasKind()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        spec.Resource.Body.Should().Contain(p => p.Key == "kind")
            .Which.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("app,linux,container");
    }

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_SiteConfigHasAcrMiProps()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var siteConfig = properties.Properties.Should().Contain(p => p.Key == "siteConfig")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        siteConfig.Properties.Should().Contain(p => p.Key == "acrUseManagedIdentityCreds")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("acrUseManagedIdentityCreds");

        siteConfig.Properties.Should().Contain(p => p.Key == "acrUserManagedIdentityID");
    }

    [Fact]
    public void Given_ContainerMiResource_When_GenerateSpec_Then_AcrUserManagedIdentityIdIsConditional()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var siteConfig = properties.Properties.Should().Contain(p => p.Key == "siteConfig")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var acrUaiId = siteConfig.Properties.Should().Contain(p => p.Key == "acrUserManagedIdentityID")
            .Which.Value.Should().BeOfType<BicepConditionalExpression>().Subject;

        acrUaiId.Condition.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("!empty(acrUserManagedIdentityId)");
    }

    // ── Container Admin variant: Module identity ──

    [Fact]
    public void Given_ContainerAdminResource_When_GenerateSpec_Then_ModuleFileNameIsContainerAdmin()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());

        spec.ModuleName.Should().Be("webApp");
        spec.ModuleFileName.Should().Be("webAppContainerAdminCredentials");
    }

    // ── Container Admin variant: Params ──

    [Fact]
    public void Given_ContainerAdminResource_When_GenerateSpec_Then_HasThirteenParams()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());
        spec.Parameters.Should().HaveCount(13);
    }

    [Fact]
    public void Given_ContainerAdminResource_When_GenerateSpec_Then_HasSecureAcrPasswordParam()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "acrPassword").Which;

        param.IsSecure.Should().BeTrue();
        param.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_ContainerAdminResource_When_GenerateSpec_Then_HasNoManagedIdentityParams()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());
        spec.Parameters.Should().NotContain(p => p.Name == "acrUseManagedIdentityCreds");
        spec.Parameters.Should().NotContain(p => p.Name == "acrUserManagedIdentityId");
    }

    // ── Container Admin variant: Variables ──

    [Fact]
    public void Given_ContainerAdminResource_When_GenerateSpec_Then_HasDockerImageAndAcrUsernameVars()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());
        spec.Variables.Should().HaveCount(2);
        spec.Variables.Should().Contain(v => v.Name == "dockerImage");
        spec.Variables.Should().Contain(v => v.Name == "acrUsername");
    }

    // ── Container Admin variant: Resource ──

    [Fact]
    public void Given_ContainerAdminResource_When_GenerateSpec_Then_SiteConfigHasAcrUseManagedIdentityCredsFalse()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var siteConfig = properties.Properties.Should().Contain(p => p.Key == "siteConfig")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        siteConfig.Properties.Should().Contain(p => p.Key == "acrUseManagedIdentityCreds")
            .Which.Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_ContainerAdminResource_When_GenerateSpec_Then_SiteConfigHasAppSettingsArray()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var siteConfig = properties.Properties.Should().Contain(p => p.Key == "siteConfig")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var appSettings = siteConfig.Properties.Should().Contain(p => p.Key == "appSettings")
            .Which.Value.Should().BeOfType<BicepArrayExpression>().Subject;

        appSettings.Items.Should().HaveCount(3);
    }

    [Fact]
    public void Given_ContainerAdminResource_When_GenerateSpec_Then_AppSettingsHasDockerRegistryEntries()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var siteConfig = properties.Properties.Should().Contain(p => p.Key == "siteConfig")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var appSettings = siteConfig.Properties.Should().Contain(p => p.Key == "appSettings")
            .Which.Value.Should().BeOfType<BicepArrayExpression>().Subject;

        // First entry: DOCKER_REGISTRY_SERVER_URL
        var url = appSettings.Items[0].Should().BeOfType<BicepObjectExpression>().Subject;
        url.Properties.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("DOCKER_REGISTRY_SERVER_URL");

        // Second entry: DOCKER_REGISTRY_SERVER_USERNAME
        var username = appSettings.Items[1].Should().BeOfType<BicepObjectExpression>().Subject;
        username.Properties.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("DOCKER_REGISTRY_SERVER_USERNAME");

        // Third entry: DOCKER_REGISTRY_SERVER_PASSWORD
        var password = appSettings.Items[2].Should().BeOfType<BicepObjectExpression>().Subject;
        password.Properties.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("DOCKER_REGISTRY_SERVER_PASSWORD");
    }

    // ── Legacy compatibility ──

    [Fact]
    public void Given_Generator_Then_AlsoImplementsIResourceTypeBicepGenerator()
    {
        _sut.Should().BeAssignableTo<IResourceTypeBicepGenerator>();
    }

    [Fact]
    public void Given_CodeResource_When_LegacyGenerate_Then_ModuleFileNameIsWebApp()
    {
        var module = _sut.Generate(CreateCodeResource());
        module.ModuleFileName.Should().Be("webApp.module.bicep");
    }

    [Fact]
    public void Given_ContainerMiResource_When_LegacyGenerate_Then_ModuleFileNameIsContainerMi()
    {
        var module = _sut.Generate(CreateContainerMiResource());
        module.ModuleFileName.Should().Be("webAppContainerManagedIdentity.module.bicep");
    }

    [Fact]
    public void Given_ContainerAdminResource_When_LegacyGenerate_Then_ModuleFileNameIsContainerAdmin()
    {
        var module = _sut.Generate(CreateContainerAdminResource());
        module.ModuleFileName.Should().Be("webAppContainerAdminCredentials.module.bicep");
    }

    [Fact]
    public void Given_ContainerAdminResource_When_LegacyGenerate_Then_AcrPasswordIsSecure()
    {
        var module = _sut.Generate(CreateContainerAdminResource());
        module.SecureParameters.Should().Contain("acrPassword");
    }

    // ── Emission: Code variant ──

    [Fact]
    public void Given_CodeResource_When_EmitModule_Then_ContainsImport()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("import { RuntimeStack } from './types.bicep'");
    }

    [Fact]
    public void Given_CodeResource_When_EmitModule_Then_ContainsAllParams()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param appServicePlanId string");
        bicep.Should().Contain("param runtimeStack RuntimeStack = 'DOTNETCORE'");
        bicep.Should().Contain("param runtimeVersion string");
        bicep.Should().Contain("param alwaysOn bool");
        bicep.Should().Contain("param httpsOnly bool");
        bicep.Should().Contain("param deploymentMode string = 'Code'");
        bicep.Should().Contain("param customDomains array = []");
    }

    [Fact]
    public void Given_CodeResource_When_EmitModule_Then_ContainsLinuxFxVersionVar()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("var linuxFxVersion =");
    }

    [Fact]
    public void Given_CodeResource_When_EmitModule_Then_ContainsResourceDeclaration()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("resource webApp 'Microsoft.Web/sites@2023-12-01' = {");
    }

    [Fact]
    public void Given_CodeResource_When_EmitModule_Then_ContainsHostNameBindingsForLoop()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("resource hostNameBindings 'Microsoft.Web/sites/hostNameBindings@2023-12-01' = [for domain in customDomains: {");
        bicep.Should().Contain("parent: webApp");
        bicep.Should().Contain("name: domain.domainName");
        bicep.Should().Contain("}]");
    }

    [Fact]
    public void Given_CodeResource_When_EmitModule_Then_ContainsAllOutputs()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("output id string = webApp.id");
        bicep.Should().Contain("output defaultHostName string = webApp.properties.defaultHostName");
        bicep.Should().Contain("output principalId string = webApp.identity.principalId");
        bicep.Should().Contain("output customDomainVerificationId string = webApp.properties.customDomainVerificationId");
    }

    // ── Emission: Container MI variant ──

    [Fact]
    public void Given_ContainerMiResource_When_EmitModule_Then_ContainsContainerParams()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("param dockerImageName string");
        bicep.Should().Contain("param dockerImageTag string = 'latest'");
        bicep.Should().Contain("param acrLoginServer string");
        bicep.Should().Contain("param acrUseManagedIdentityCreds bool = true");
        bicep.Should().Contain("param acrUserManagedIdentityId string = ''");
        bicep.Should().Contain("param deploymentMode string = 'Container'");
    }

    [Fact]
    public void Given_ContainerMiResource_When_EmitModule_Then_ContainsKind()
    {
        var spec = _sut.GenerateSpec(CreateContainerMiResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("kind: 'app,linux,container'");
    }

    // ── Emission: Container Admin variant ──

    [Fact]
    public void Given_ContainerAdminResource_When_EmitModule_Then_ContainsSecureAcrPassword()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("@secure()");
        bicep.Should().Contain("param acrPassword string");
    }

    [Fact]
    public void Given_ContainerAdminResource_When_EmitModule_Then_ContainsAppSettings()
    {
        var spec = _sut.GenerateSpec(CreateContainerAdminResource());
        var bicep = new BicepEmitter().EmitModule(spec);

        bicep.Should().Contain("DOCKER_REGISTRY_SERVER_URL");
        bicep.Should().Contain("DOCKER_REGISTRY_SERVER_USERNAME");
        bicep.Should().Contain("DOCKER_REGISTRY_SERVER_PASSWORD");
    }

    // ── Emission: Types ──

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsBothTypes()
    {
        var spec = _sut.GenerateSpec(CreateCodeResource());
        var types = new BicepEmitter().EmitTypes(spec);

        types.Should().Contain("type RuntimeStack = 'DOTNETCORE' | 'NODE' | 'PYTHON' | 'JAVA' | 'PHP'");
        types.Should().Contain("type DeploymentMode = 'Code' | 'Container'");
    }
}
