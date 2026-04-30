using System.Text.Json;
using FluentAssertions;
using InfraFlowSculptor.Mcp.Tools;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

public class DiscoveryToolsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    // ── list_repository_topologies ──────────────────────────────────────

    [Fact]
    public void ListRepositoryTopologies_ReturnsAllThreeTopologies()
    {
        // Act
        var json = DiscoveryTools.ListRepositoryTopologies();
        var result = JsonSerializer.Deserialize<TopologiesResponse>(json, JsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Topologies.Should().HaveCount(3);
    }

    [Fact]
    public void ListRepositoryTopologies_AllInOne_HasCorrectStructure()
    {
        var json = DiscoveryTools.ListRepositoryTopologies();
        var result = JsonSerializer.Deserialize<TopologiesResponse>(json, JsonOptions)!;

        var allInOne = result.Topologies.Should().ContainSingle(t => t.Id == "AllInOne").Subject;

        allInOne.Label.Should().Be("All-in-One (Mono Repo)");
        allInOne.RequiredRepositoryCount.Should().Be(1);
        allInOne.RepositoryContentKinds.Should().HaveCount(1);
        allInOne.RepositoryContentKinds[0].Should().BeEquivalentTo(new[] { "Infrastructure", "Application" });
    }

    [Fact]
    public void ListRepositoryTopologies_SplitInfraCode_HasCorrectStructure()
    {
        var json = DiscoveryTools.ListRepositoryTopologies();
        var result = JsonSerializer.Deserialize<TopologiesResponse>(json, JsonOptions)!;

        var split = result.Topologies.Should().ContainSingle(t => t.Id == "SplitInfraCode").Subject;

        split.Label.Should().Be("Split Infra / Code");
        split.RequiredRepositoryCount.Should().Be(2);
        split.RepositoryContentKinds.Should().HaveCount(2);
        split.RepositoryContentKinds[0].Should().BeEquivalentTo(new[] { "Infrastructure" });
        split.RepositoryContentKinds[1].Should().BeEquivalentTo(new[] { "Application" });
    }

    [Fact]
    public void ListRepositoryTopologies_MultiRepo_HasCorrectStructure()
    {
        var json = DiscoveryTools.ListRepositoryTopologies();
        var result = JsonSerializer.Deserialize<TopologiesResponse>(json, JsonOptions)!;

        var multi = result.Topologies.Should().ContainSingle(t => t.Id == "MultiRepo").Subject;

        multi.Label.Should().Be("Multi-Repo");
        multi.RequiredRepositoryCount.Should().Be(0);
        multi.RepositoryContentKinds.Should().BeEmpty();
    }

    // ── list_supported_resource_types ───────────────────────────────────

    [Fact]
    public void ListSupportedResourceTypes_ReturnsAllEighteenTypes()
    {
        var json = DiscoveryTools.ListSupportedResourceTypes();
        var result = JsonSerializer.Deserialize<ResourceTypesResponse>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.ResourceTypes.Should().HaveCount(18);
    }

    [Fact]
    public void ListSupportedResourceTypes_EachType_HasRequiredFields()
    {
        var json = DiscoveryTools.ListSupportedResourceTypes();
        var result = JsonSerializer.Deserialize<ResourceTypesResponse>(json, JsonOptions)!;

        foreach (var rt in result.ResourceTypes)
        {
            rt.Id.Should().NotBeNullOrWhiteSpace($"resource type should have an id");
            rt.ArmType.Should().NotBeNullOrWhiteSpace($"{rt.Id} should have an armType");
            rt.Label.Should().NotBeNullOrWhiteSpace($"{rt.Id} should have a label");
            rt.Category.Should().NotBeNullOrWhiteSpace($"{rt.Id} should have a category");
        }
    }

    [Fact]
    public void ListSupportedResourceTypes_DoesNotIncludeResourceGroup()
    {
        var json = DiscoveryTools.ListSupportedResourceTypes();
        var result = JsonSerializer.Deserialize<ResourceTypesResponse>(json, JsonOptions)!;

        result.ResourceTypes.Should().NotContain(rt => rt.Id == "ResourceGroup");
    }

    [Fact]
    public void ListSupportedResourceTypes_KeyVault_HasCorrectMapping()
    {
        var json = DiscoveryTools.ListSupportedResourceTypes();
        var result = JsonSerializer.Deserialize<ResourceTypesResponse>(json, JsonOptions)!;

        var kv = result.ResourceTypes.Should().ContainSingle(rt => rt.Id == "KeyVault").Subject;

        kv.ArmType.Should().Be("Microsoft.KeyVault/vaults");
        kv.Label.Should().Be("Azure Key Vault");
        kv.Category.Should().Be("Security");
    }

    // ── Deserialization DTOs (test-only) ───────────────────────────────

    private sealed class TopologiesResponse
    {
        public List<TopologyDto> Topologies { get; set; } = [];
    }

    private sealed class TopologyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RequiredRepositoryCount { get; set; }
        public List<List<string>> RepositoryContentKinds { get; set; } = [];
    }

    private sealed class ResourceTypesResponse
    {
        public List<ResourceTypeDto> ResourceTypes { get; set; } = [];
    }

    private sealed class ResourceTypeDto
    {
        public string Id { get; set; } = string.Empty;
        public string ArmType { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
