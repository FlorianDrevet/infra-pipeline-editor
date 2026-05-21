using FluentAssertions;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Drafts.Models;
using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Mcp.Tests.Drafts;

public sealed class ProjectDraftServiceStackDetectionTests
{
    private readonly ProjectDraftService _sut = new(
        Options.Create(new ProjectDraftStorageOptions { MaxDraftCount = 50 }));

    // ── ExtractApplicationStack from prompt ──────────────────────────────────

    [Theory]
    [InlineData("Create a .NET API project with a Container App", "DotNet")]
    [InlineData("I want a dotnet backend with Redis", "DotNet")]
    [InlineData("Create a C# microservice with ACA", "DotNet")]
    [InlineData("Build me an ASP.NET web app", "DotNet")]
    public void Given_DotNetKeywords_When_CreateDraft_Then_ComputeResourcesHaveDotNetStack(string prompt, string expectedStack)
    {
        // Act
        var draft = _sut.CreateDraftFromPrompt($"{prompt} mono repo");

        // Assert
        var computeResources = draft.Intent.Resources!
            .Where(r => r.ResourceType is AzureResourceTypes.ContainerApp or AzureResourceTypes.WebApp or AzureResourceTypes.FunctionApp);
        computeResources.Should().AllSatisfy(r => r.ApplicationStack.Should().Be(expectedStack));
    }

    [Theory]
    [InlineData("Create a NodeJS API with a Container App", "NodeJs")]
    [InlineData("I need an express backend on webapp", "NodeJs")]
    [InlineData("npm project on container app", "NodeJs")]
    public void Given_NodeJsKeywords_When_CreateDraft_Then_ComputeResourcesHaveNodeJsStack(string prompt, string expectedStack)
    {
        // Act
        var draft = _sut.CreateDraftFromPrompt($"{prompt} mono repo");

        // Assert
        var computeResources = draft.Intent.Resources!
            .Where(r => r.ResourceType is AzureResourceTypes.ContainerApp or AzureResourceTypes.WebApp or AzureResourceTypes.FunctionApp);
        computeResources.Should().AllSatisfy(r => r.ApplicationStack.Should().Be(expectedStack));
    }

    [Theory]
    [InlineData("Create an Angular frontend on container app", "Angular")]
    [InlineData("I need a web app for my angular project", "Angular")]
    public void Given_AngularKeywords_When_CreateDraft_Then_ComputeResourcesHaveAngularStack(string prompt, string expectedStack)
    {
        // Act
        var draft = _sut.CreateDraftFromPrompt($"{prompt} mono repo");

        // Assert
        var computeResources = draft.Intent.Resources!
            .Where(r => r.ResourceType is AzureResourceTypes.ContainerApp or AzureResourceTypes.WebApp or AzureResourceTypes.FunctionApp);
        computeResources.Should().AllSatisfy(r => r.ApplicationStack.Should().Be(expectedStack));
    }

    [Theory]
    [InlineData("Deploy a Java Spring Boot service on container app", "Java")]
    [InlineData("I need a Maven project on webapp", "Java")]
    public void Given_JavaKeywords_When_CreateDraft_Then_ComputeResourcesHaveJavaStack(string prompt, string expectedStack)
    {
        // Act
        var draft = _sut.CreateDraftFromPrompt($"{prompt} mono repo");

        // Assert
        var computeResources = draft.Intent.Resources!
            .Where(r => r.ResourceType is AzureResourceTypes.ContainerApp or AzureResourceTypes.WebApp or AzureResourceTypes.FunctionApp);
        computeResources.Should().AllSatisfy(r => r.ApplicationStack.Should().Be(expectedStack));
    }

    [Theory]
    [InlineData("Deploy a Python FastAPI on container app", "Python")]
    [InlineData("Flask application on webapp", "Python")]
    public void Given_PythonKeywords_When_CreateDraft_Then_ComputeResourcesHavePythonStack(string prompt, string expectedStack)
    {
        // Act
        var draft = _sut.CreateDraftFromPrompt($"{prompt} mono repo");

        // Assert
        var computeResources = draft.Intent.Resources!
            .Where(r => r.ResourceType is AzureResourceTypes.ContainerApp or AzureResourceTypes.WebApp or AzureResourceTypes.FunctionApp);
        computeResources.Should().AllSatisfy(r => r.ApplicationStack.Should().Be(expectedStack));
    }

    [Theory]
    [InlineData("Deploy a static site on web app", "StaticSite")]
    [InlineData("Hugo blog on webapp", "StaticSite")]
    public void Given_StaticSiteKeywords_When_CreateDraft_Then_ComputeResourcesHaveStaticSiteStack(string prompt, string expectedStack)
    {
        // Act
        var draft = _sut.CreateDraftFromPrompt($"{prompt} mono repo");

        // Assert
        var computeResources = draft.Intent.Resources!
            .Where(r => r.ResourceType is AzureResourceTypes.ContainerApp or AzureResourceTypes.WebApp or AzureResourceTypes.FunctionApp);
        computeResources.Should().AllSatisfy(r => r.ApplicationStack.Should().Be(expectedStack));
    }

    // ── Clarification question when no stack detected ──────────────────────

    [Fact]
    public void Given_ComputeResourceWithoutStackKeywords_When_CreateDraft_Then_HasApplicationStackClarification()
    {
        // Arrange
        const string prompt = "Create project MyApp mono repo with a container app";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.ClarificationQuestions.Should().Contain(q => q.Field == ProjectDraftService.DraftFieldNames.ApplicationStack);
        var stackQuestion = draft.ClarificationQuestions.First(q => q.Field == ProjectDraftService.DraftFieldNames.ApplicationStack);
        stackQuestion.Options.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void Given_ComputeResourceWithStackDetected_When_CreateDraft_Then_NoApplicationStackClarification()
    {
        // Arrange
        const string prompt = "Create project MyApp mono repo with a dotnet container app";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.ClarificationQuestions.Should().NotContain(q => q.Field == ProjectDraftService.DraftFieldNames.ApplicationStack);
    }

    [Fact]
    public void Given_NonComputeResourcesOnly_When_CreateDraft_Then_NoApplicationStackClarification()
    {
        // Arrange
        const string prompt = "Create project MyApp mono repo with a key vault and storage account";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.ClarificationQuestions.Should().NotContain(q => q.Field == ProjectDraftService.DraftFieldNames.ApplicationStack);
    }

    [Fact]
    public void Given_NonComputeResources_When_CreateDraft_Then_ApplicationStackIsNull()
    {
        // Arrange
        const string prompt = "Create a dotnet project MyApp mono repo with a key vault";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        var keyVault = draft.Intent.Resources!.First(r => r.ResourceType == AzureResourceTypes.KeyVault);
        keyVault.ApplicationStack.Should().BeNull();
    }

    // ── Override application stack via validate ──────────────────────────────

    [Fact]
    public void Given_DraftWithComputeResourceNoStack_When_OverrideApplicationStack_Then_StackApplied()
    {
        // Arrange
        var draft = _sut.CreateDraftFromPrompt("Create project MyApp mono repo with a container app");
        draft.Intent.Resources!.First(r => r.ResourceType == AzureResourceTypes.ContainerApp)
            .ApplicationStack.Should().BeNull();

        var overrides = new DraftOverrides
        {
            ApplicationStack = "NodeJs",
        };

        // Act
        var updated = _sut.ValidateAndUpdate(draft.DraftId, overrides);

        // Assert
        updated.Should().NotBeNull();
        updated!.Intent.Resources!.First(r => r.ResourceType == AzureResourceTypes.ContainerApp)
            .ApplicationStack.Should().Be("NodeJs");
        updated.ClarificationQuestions.Should().NotContain(q => q.Field == ProjectDraftService.DraftFieldNames.ApplicationStack);
    }

    [Fact]
    public void Given_DraftWithComputeResource_When_OverrideWithInvalidStack_Then_StackNotApplied()
    {
        // Arrange
        var draft = _sut.CreateDraftFromPrompt("Create project MyApp mono repo with a container app");

        var overrides = new DraftOverrides
        {
            ApplicationStack = "InvalidStack",
        };

        // Act
        var updated = _sut.ValidateAndUpdate(draft.DraftId, overrides);

        // Assert
        updated.Should().NotBeNull();
        updated!.Intent.Resources!.First(r => r.ResourceType == AzureResourceTypes.ContainerApp)
            .ApplicationStack.Should().BeNull();
    }
}
