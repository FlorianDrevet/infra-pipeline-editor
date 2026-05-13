using System.Xml.Linq;
using FluentAssertions;

namespace InfraFlowSculptor.Mcp.Tests.Topology;

public sealed class McpProjectTopologyTests
{
    private const string InfraFlowSculptorApiProjectFileName = "InfraFlowSculptor.Api.csproj";

    [Fact]
    public void Given_McpProjectFile_When_InspectingProjectReferences_Then_DoesNotReferenceApiDirectly()
    {
        // Arrange
        var projectFilePath = Path.Combine(GetRepositoryRoot(), "src", "Mcp", "InfraFlowSculptor.Mcp", "InfraFlowSculptor.Mcp.csproj");

        // Act
        var projectDocument = XDocument.Load(projectFilePath);
        var projectReferences = projectDocument
            .Descendants("ProjectReference")
            .Select(reference => (string?)reference.Attribute("Include"))
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Cast<string>()
            .ToArray();

        // Assert
        projectReferences.Should().NotContain(reference => reference.EndsWith(InfraFlowSculptorApiProjectFileName, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}