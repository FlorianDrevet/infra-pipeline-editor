using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations;

public sealed class CoreStringLengthConfigurationTests
{
    private const int ProjectNameMaxLength = 80;
    private const int InfrastructureConfigNameMaxLength = 100;
    private const int AzureResourceNameMaxLength = 260;
    private const int NamingTemplateMaxLength = 500;
    private const int ProjectEnvironmentNameMaxLength = 100;
    private const int ProjectEnvironmentShortNameMaxLength = 20;
    private const int ProjectEnvironmentAffixMaxLength = 50;
    private const int ResourceGroupNameMaxLength = 90;
    private const int ParameterDefinitionNameMaxLength = 100;
    private const int ParameterDefinitionTypeMaxLength = 20;
    private const int ParameterDefinitionDefaultValueMaxLength = 500;

    [Fact]
    public void Given_ProjectEntityModel_When_InspectingCoreStringProperties_Then_UsesExpectedMaxLengths()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.Create();

        // Act
        var nameProperty = GetProperty<Project>(context, nameof(Project.Name));
        var descriptionProperty = GetProperty<Project>(context, nameof(Project.Description));
        var defaultNamingTemplateProperty = GetProperty<Project>(context, nameof(Project.DefaultNamingTemplate));
        var agentPoolNameProperty = GetProperty<Project>(context, nameof(Project.AgentPoolName));

        // Assert
        nameProperty.GetMaxLength().Should().Be(ProjectNameMaxLength);
        descriptionProperty.GetMaxLength().Should().Be(500);
        defaultNamingTemplateProperty.GetMaxLength().Should().Be(NamingTemplateMaxLength);
        agentPoolNameProperty.GetMaxLength().Should().Be(200);
    }

    [Fact]
    public void Given_ProjectEnvironmentDefinitionOwnedModel_When_InspectingCoreStringProperties_Then_UsesExpectedMaxLengths()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.Create();

        // Act
        var nameProperty = GetOwnedProperty<ProjectEnvironmentDefinition>(context, nameof(ProjectEnvironmentDefinition.Name));
        var shortNameProperty = GetOwnedProperty<ProjectEnvironmentDefinition>(context, nameof(ProjectEnvironmentDefinition.ShortName));
        var prefixProperty = GetOwnedProperty<ProjectEnvironmentDefinition>(context, nameof(ProjectEnvironmentDefinition.Prefix));
        var suffixProperty = GetOwnedProperty<ProjectEnvironmentDefinition>(context, nameof(ProjectEnvironmentDefinition.Suffix));
        var azureResourceManagerConnectionProperty = GetOwnedProperty<ProjectEnvironmentDefinition>(
            context,
            nameof(ProjectEnvironmentDefinition.AzureResourceManagerConnection));

        // Assert
        nameProperty.GetMaxLength().Should().Be(ProjectEnvironmentNameMaxLength);
        shortNameProperty.GetMaxLength().Should().Be(ProjectEnvironmentShortNameMaxLength);
        prefixProperty.GetMaxLength().Should().Be(ProjectEnvironmentAffixMaxLength);
        suffixProperty.GetMaxLength().Should().Be(ProjectEnvironmentAffixMaxLength);
        azureResourceManagerConnectionProperty.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void Given_InfrastructureConfigEntityModel_When_InspectingCoreStringProperties_Then_UsesExpectedMaxLengths()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.Create();

        // Act
        var nameProperty = GetProperty<InfrastructureConfig>(context, nameof(InfrastructureConfig.Name));
        var defaultNamingTemplateProperty = GetProperty<InfrastructureConfig>(context, nameof(InfrastructureConfig.DefaultNamingTemplate));
        var appPipelineModeProperty = GetProperty<InfrastructureConfig>(context, nameof(InfrastructureConfig.AppPipelineMode));
        var layoutModeProperty = GetProperty<InfrastructureConfig>(context, nameof(InfrastructureConfig.LayoutMode));

        // Assert
        nameProperty.GetMaxLength().Should().Be(InfrastructureConfigNameMaxLength);
        defaultNamingTemplateProperty.GetMaxLength().Should().Be(NamingTemplateMaxLength);
        appPipelineModeProperty.GetMaxLength().Should().Be(20);
        layoutModeProperty.GetMaxLength().Should().Be(30);
    }

    [Fact]
    public void Given_AzureResourceEntityModel_When_InspectingCoreStringProperties_Then_UsesExpectedMaxLengths()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.Create();

        // Act
        var nameProperty = GetProperty<InfraFlowSculptor.Domain.Common.BaseModels.AzureResource>(context, "Name");
        var customNameOverrideProperty = GetProperty<InfraFlowSculptor.Domain.Common.BaseModels.AzureResource>(context, "CustomNameOverride");
        var resourceTypeProperty = GetProperty<InfraFlowSculptor.Domain.Common.BaseModels.AzureResource>(context, "ResourceType");

        // Assert
        nameProperty.GetMaxLength().Should().Be(AzureResourceNameMaxLength);
        customNameOverrideProperty.GetMaxLength().Should().Be(AzureResourceNameMaxLength);
        resourceTypeProperty.GetMaxLength().Should().Be(50);
    }

    [Fact]
    public void Given_ResourceNamingTemplateModels_When_InspectingTemplateProperties_Then_UseExpectedMaxLengths()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.Create();

        // Act
        var projectTemplateProperty = GetProperty<ProjectResourceNamingTemplate>(context, nameof(ProjectResourceNamingTemplate.Template));
        var projectResourceTypeProperty = GetProperty<ProjectResourceNamingTemplate>(context, nameof(ProjectResourceNamingTemplate.ResourceType));
        var infraTemplateProperty = GetProperty<ResourceNamingTemplate>(context, nameof(ResourceNamingTemplate.Template));
        var infraResourceTypeProperty = GetProperty<ResourceNamingTemplate>(context, nameof(ResourceNamingTemplate.ResourceType));

        // Assert
        projectTemplateProperty.GetMaxLength().Should().Be(NamingTemplateMaxLength);
        projectResourceTypeProperty.GetMaxLength().Should().Be(100);
        infraTemplateProperty.GetMaxLength().Should().Be(NamingTemplateMaxLength);
        infraResourceTypeProperty.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void Given_ResourceGroupEntityModel_When_InspectingNameProperty_Then_UsesExpectedMaxLength()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.Create();

        // Act
        var nameProperty = GetProperty<ResourceGroup>(context, nameof(ResourceGroup.Name));

        // Assert
        nameProperty.GetMaxLength().Should().Be(ResourceGroupNameMaxLength);
    }

    [Fact]
    public void Given_ParameterDefinitionEntityModel_When_InspectingStringProperties_Then_UsesExpectedMaxLengths()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.Create();

        // Act
        var nameProperty = GetProperty<ParameterDefinition>(context, nameof(ParameterDefinition.Name));
        var typeProperty = GetProperty<ParameterDefinition>(context, nameof(ParameterDefinition.Type));
        var defaultValueProperty = GetProperty<ParameterDefinition>(context, nameof(ParameterDefinition.DefaultValue));

        // Assert
        nameProperty.GetMaxLength().Should().Be(ParameterDefinitionNameMaxLength);
        typeProperty.GetMaxLength().Should().Be(ParameterDefinitionTypeMaxLength);
        defaultValueProperty.GetMaxLength().Should().Be(ParameterDefinitionDefaultValueMaxLength);
    }

    private static IProperty GetProperty<TEntity>(ProjectDbContext context, string propertyName)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        entityType.Should().NotBeNull();

        var property = entityType!.FindProperty(propertyName);
        property.Should().NotBeNull();
        return property!;
    }

    private static IProperty GetOwnedProperty<TEntity>(ProjectDbContext context, string propertyName)
    {
        var entityType = context.Model.GetEntityTypes().Single(type => type.ClrType == typeof(TEntity));

        var property = entityType.FindProperty(propertyName);
        property.Should().NotBeNull();
        return property!;
    }
}