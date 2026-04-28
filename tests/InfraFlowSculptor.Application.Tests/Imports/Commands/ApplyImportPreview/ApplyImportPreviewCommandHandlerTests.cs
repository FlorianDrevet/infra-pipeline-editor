using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.Imports.Commands.ApplyImportPreview;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Imports.Commands.ApplyImportPreview;

public sealed class ApplyImportPreviewCommandHandlerTests
{
    private readonly ISender _mediator;
    private readonly ApplyImportPreviewCommandHandler _sut;

    public ApplyImportPreviewCommandHandlerTests()
    {
        _mediator = Substitute.For<ISender>();
        _sut = new ApplyImportPreviewCommandHandler(_mediator);
    }

    [Fact]
    public async Task Given_MappedResources_When_Handle_Then_CreatesProjectInfrastructureAndResourcesAsync()
    {
        // Arrange
        var projectId = new ProjectId(Guid.NewGuid());
        var infrastructureConfigId = new InfrastructureConfigId(Guid.NewGuid());
        var resourceGroupId = new ResourceGroupId(Guid.NewGuid());
        var keyVaultId = new AzureResourceId(Guid.NewGuid());

        _mediator.Send(Arg.Any<CreateProjectWithSetupCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateProjectResult(projectId, "MyProject"));
        _mediator.Send(Arg.Any<CreateInfrastructureConfigCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateInfrastructureConfigResult(infrastructureConfigId, projectId));
        _mediator.Send(Arg.Any<CreateResourceGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateResourceGroupResult(resourceGroupId, infrastructureConfigId));
        _mediator.Send(Arg.Any<CreateKeyVaultCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateKeyVaultResult(keyVaultId, resourceGroupId, "myKeyVault"));

        var command = CreateCommand(
            resources:
            [
                CreateMappedResource(
                    sourceType: "Microsoft.KeyVault/vaults",
                    sourceName: "myKeyVault",
                    mappedResourceType: AzureResourceTypes.KeyVault),
            ]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be("applied");
        result.Value.ProjectId.Should().Be(projectId.Value.ToString());
        result.Value.ProjectName.Should().Be("MyProject");
        result.Value.InfrastructureConfigId.Should().Be(infrastructureConfigId.Value.ToString());
        result.Value.ResourceGroupId.Should().Be(resourceGroupId.Value.ToString());
        result.Value.InfrastructureError.Should().BeNull();
        var created = result.Value.CreatedResources.Should().ContainSingle().Which;
        created.ResourceType.Should().Be(AzureResourceTypes.KeyVault);
        created.ResourceId.Should().Be(keyVaultId.Value.ToString());
        created.Name.Should().Be("myKeyVault");
        result.Value.SkippedResources.Should().BeEmpty();
        result.Value.NextSuggestedActions.Should().Contain(action => action.Contains("Generate Bicep", StringComparison.Ordinal));

        await _mediator.Received(1).Send(
            Arg.Is<CreateProjectWithSetupCommand>(request =>
                request.Name == "MyProject"
                && request.LayoutPreset == "AllInOne"
                && request.Environments.Count == 1
                && request.Environments[0].Name == "Development"
                && request.Environments[0].ShortName == "dev"
                && request.Repositories.Count == 1
                && request.Repositories[0].Alias == "main"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ResourceFilter_When_Handle_Then_OnlyCreatesMatchingResourcesAsync()
    {
        // Arrange
        var projectId = new ProjectId(Guid.NewGuid());
        var infrastructureConfigId = new InfrastructureConfigId(Guid.NewGuid());
        var resourceGroupId = new ResourceGroupId(Guid.NewGuid());
        var storageAccountId = new AzureResourceId(Guid.NewGuid());

        _mediator.Send(Arg.Any<CreateProjectWithSetupCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateProjectResult(projectId, "MyProject"));
        _mediator.Send(Arg.Any<CreateInfrastructureConfigCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateInfrastructureConfigResult(infrastructureConfigId, projectId));
        _mediator.Send(Arg.Any<CreateResourceGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateResourceGroupResult(resourceGroupId, infrastructureConfigId));
        _mediator.Send(Arg.Any<CreateStorageAccountCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateStorageAccountResult(storageAccountId, resourceGroupId, "myStorage"));

        var command = CreateCommand(
            resources:
            [
                CreateMappedResource(
                    sourceType: "Microsoft.KeyVault/vaults",
                    sourceName: "myKeyVault",
                    mappedResourceType: AzureResourceTypes.KeyVault),
                CreateMappedResource(
                    sourceType: "Microsoft.Storage/storageAccounts",
                    sourceName: "myStorage",
                    mappedResourceType: AzureResourceTypes.StorageAccount),
            ],
            resourceFilter: ["myStorage"]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.CreatedResources.Should().ContainSingle(resource =>
            resource.ResourceType == AzureResourceTypes.StorageAccount && resource.Name == "myStorage");
        await _mediator.DidNotReceive().Send(Arg.Any<CreateKeyVaultCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ProjectCreationFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _mediator.Send(Arg.Any<CreateProjectWithSetupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResult>>(Error.Failure(description: "Validation failed")));

        var command = CreateCommand(
            resources:
            [
                CreateMappedResource(
                    sourceType: "Microsoft.KeyVault/vaults",
                    sourceName: "myKeyVault",
                    mappedResourceType: AzureResourceTypes.KeyVault),
            ]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Be("Validation failed");
        await _mediator.DidNotReceive().Send(Arg.Any<CreateInfrastructureConfigCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_InfrastructureCreationFails_When_Handle_Then_ReturnsPartialSuccessWithInfrastructureErrorAsync()
    {
        // Arrange
        var projectId = new ProjectId(Guid.NewGuid());

        _mediator.Send(Arg.Any<CreateProjectWithSetupCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateProjectResult(projectId, "MyProject"));
        _mediator.Send(Arg.Any<CreateInfrastructureConfigCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<GetInfrastructureConfigResult>>(Error.Failure(description: "Infrastructure creation failed")));

        var command = CreateCommand(
            resources:
            [
                CreateMappedResource(
                    sourceType: "Microsoft.KeyVault/vaults",
                    sourceName: "myKeyVault",
                    mappedResourceType: AzureResourceTypes.KeyVault),
            ]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ProjectId.Should().Be(projectId.Value.ToString());
        result.Value.InfrastructureConfigId.Should().BeNull();
        result.Value.ResourceGroupId.Should().BeNull();
        result.Value.InfrastructureError.Should().Contain("Infrastructure creation failed");
        result.Value.CreatedResources.Should().BeEmpty();
        result.Value.SkippedResources.Should().BeEmpty();
        result.Value.NextSuggestedActions.Should().Contain(action => action.Contains("Create an infrastructure configuration manually", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Given_DependentResourceWithoutPrerequisite_When_Handle_Then_SkipsResourceAsync()
    {
        // Arrange
        var projectId = new ProjectId(Guid.NewGuid());
        var infrastructureConfigId = new InfrastructureConfigId(Guid.NewGuid());
        var resourceGroupId = new ResourceGroupId(Guid.NewGuid());

        _mediator.Send(Arg.Any<CreateProjectWithSetupCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateProjectResult(projectId, "MyProject"));
        _mediator.Send(Arg.Any<CreateInfrastructureConfigCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateInfrastructureConfigResult(infrastructureConfigId, projectId));
        _mediator.Send(Arg.Any<CreateResourceGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateResourceGroupResult(resourceGroupId, infrastructureConfigId));

        var command = CreateCommand(
            resources:
            [
                CreateMappedResource(
                    sourceType: "Microsoft.Web/sites",
                    sourceName: "myWebApp",
                    mappedResourceType: AzureResourceTypes.WebApp),
            ]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.CreatedResources.Should().BeEmpty();
        var skipped = result.Value.SkippedResources.Should().ContainSingle().Which;
        skipped.ResourceType.Should().Be(AzureResourceTypes.WebApp);
        skipped.Name.Should().Be("myWebApp");
        skipped.Reason.Should().Contain(AzureResourceTypes.AppServicePlan);
    }

    [Fact]
    public async Task Given_NoMappedResources_When_Handle_Then_CreatesProjectWithoutInfrastructureAsync()
    {
        // Arrange
        var projectId = new ProjectId(Guid.NewGuid());

        _mediator.Send(Arg.Any<CreateProjectWithSetupCommand>(), Arg.Any<CancellationToken>())
            .Returns(CreateProjectResult(projectId, "MyProject"));

        var command = CreateCommand(
            resources:
            [
                new ImportedResourceAnalysisResult
                {
                    SourceType = "Microsoft.Unknown/resource",
                    SourceName = "legacy-resource",
                    Confidence = ImportPreviewMappingConfidence.Low,
                },
            ]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ProjectId.Should().Be(projectId.Value.ToString());
        result.Value.InfrastructureConfigId.Should().BeNull();
        result.Value.ResourceGroupId.Should().BeNull();
        result.Value.CreatedResources.Should().BeEmpty();
        result.Value.SkippedResources.Should().BeEmpty();
        result.Value.NextSuggestedActions.Should().Contain(action => action.Contains("No mapped resources found", StringComparison.Ordinal));
        await _mediator.DidNotReceive().Send(Arg.Any<CreateInfrastructureConfigCommand>(), Arg.Any<CancellationToken>());
    }

    private static ApplyImportPreviewCommand CreateCommand(
        IReadOnlyList<ImportedResourceAnalysisResult> resources,
        IReadOnlyList<string>? resourceFilter = null,
        IReadOnlyList<EnvironmentSetupItem>? environments = null)
    {
        return new ApplyImportPreviewCommand(
            ProjectName: "MyProject",
            LayoutPreset: "AllInOne",
            Preview: new ImportPreviewAnalysisResult
            {
                SourceFormat = "arm-json",
                Resources = resources,
                Dependencies = [],
                Metadata = new Dictionary<string, string>(),
                Gaps = [],
                UnsupportedResources = [],
                Summary = "Test preview",
            },
            Environments: environments,
            ResourceFilter: resourceFilter);
    }

    private static ImportedResourceAnalysisResult CreateMappedResource(
        string sourceType,
        string sourceName,
        string mappedResourceType)
    {
        return new ImportedResourceAnalysisResult
        {
            SourceType = sourceType,
            SourceName = sourceName,
            MappedResourceType = mappedResourceType,
            MappedName = sourceName,
            Confidence = ImportPreviewMappingConfidence.High,
            ExtractedProperties = new Dictionary<string, object?>(),
            UnmappedProperties = [],
        };
    }

    private static Task<ErrorOr<ProjectResult>> CreateProjectResult(ProjectId projectId, string projectName)
    {
        return Task.FromResult<ErrorOr<ProjectResult>>(
            new ProjectResult(
                projectId,
                new Name(projectName),
                Description: null,
                Members: [],
                EnvironmentDefinitions: [],
                DefaultNamingTemplate: null,
                ResourceNamingTemplates: [],
                ResourceAbbreviations: [],
                Tags: [],
                LayoutPreset: "AllInOne"));
    }

    private static Task<ErrorOr<GetInfrastructureConfigResult>> CreateInfrastructureConfigResult(
        InfrastructureConfigId infrastructureConfigId,
        ProjectId projectId)
    {
        return Task.FromResult<ErrorOr<GetInfrastructureConfigResult>>(
            new GetInfrastructureConfigResult(
                infrastructureConfigId,
                new Name("MyProject-config"),
                projectId,
                DefaultNamingTemplate: null,
                UseProjectNamingConventions: false,
                ResourceNamingTemplates: [],
                ResourceAbbreviationOverrides: [],
                Tags: [],
                ResourceGroupCount: 0,
                ResourceCount: 0,
                CrossConfigReferenceCount: 0,
                LayoutMode: null,
                Repositories: null));
    }

    private static Task<ErrorOr<ResourceGroupResult>> CreateResourceGroupResult(
        ResourceGroupId resourceGroupId,
        InfrastructureConfigId infrastructureConfigId)
    {
        return Task.FromResult<ErrorOr<ResourceGroupResult>>(
            new ResourceGroupResult(
                resourceGroupId,
                infrastructureConfigId,
                new Location(Location.LocationEnum.WestEurope),
                new Name("MyProject-rg"),
                []));
    }

    private static Task<ErrorOr<KeyVaultResult>> CreateKeyVaultResult(
        AzureResourceId resourceId,
        ResourceGroupId resourceGroupId,
        string resourceName)
    {
        return Task.FromResult<ErrorOr<KeyVaultResult>>(
            new KeyVaultResult(
                resourceId,
                resourceGroupId,
                new Name(resourceName),
                new Location(Location.LocationEnum.WestEurope),
                EnableRbacAuthorization: false,
                EnabledForDeployment: false,
                EnabledForDiskEncryption: false,
                EnabledForTemplateDeployment: false,
                EnablePurgeProtection: false,
                EnableSoftDelete: false,
                EnvironmentSettings: []));
    }

    private static Task<ErrorOr<StorageAccountResult>> CreateStorageAccountResult(
        AzureResourceId resourceId,
        ResourceGroupId resourceGroupId,
        string resourceName)
    {
        return Task.FromResult<ErrorOr<StorageAccountResult>>(
            new StorageAccountResult(
                resourceId,
                resourceGroupId,
                new Name(resourceName),
                new Location(Location.LocationEnum.WestEurope),
                Kind: "StorageV2",
                AccessTier: "Hot",
                AllowBlobPublicAccess: false,
                EnableHttpsTrafficOnly: true,
                MinimumTlsVersion: "TLS1_2",
                CorsRules: [],
                TableCorsRules: [],
                BlobContainers: [],
                Queues: [],
                Tables: [],
                EnvironmentSettings: [],
                LifecycleRules: []));
    }
}