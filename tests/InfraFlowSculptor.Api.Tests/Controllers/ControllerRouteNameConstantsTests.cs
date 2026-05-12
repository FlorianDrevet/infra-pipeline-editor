using FluentAssertions;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Controllers;

public sealed class ControllerRouteNameConstantsTests
{
    [Fact]
    public void Given_ControllerSourceFiles_When_VerifyingRouteNamePattern_Then_AllControllersUseDedicatedRouteNameConstants()
    {
        // Arrange
        var repositoryRootPath = GetRepositoryRootPath();
        var controllersDirectoryPath = Path.Combine(repositoryRootPath, "src", "Api", "InfraFlowSculptor.Api", "Controllers");
        var constantsDirectoryPath = Path.Combine(controllersDirectoryPath, "Constants");
        var controllerFilePaths = Directory.GetFiles(controllersDirectoryPath, "*Controller.cs", SearchOption.TopDirectoryOnly);

        // Act
        var missingConstantFiles = controllerFilePaths
            .Select(controllerFilePath => GetExpectedRouteNamesFilePath(constantsDirectoryPath, controllerFilePath))
            .Where(expectedFilePath => !File.Exists(expectedFilePath))
            .Select(Path.GetFileName)
            .OrderBy(fileName => fileName)
            .ToArray();

        var controllersUsingInlineRouteNames = controllerFilePaths
            .Where(ContainsInlineRouteNameLiteral)
            .Select(Path.GetFileName)
            .OrderBy(fileName => fileName)
            .ToArray();

        // Assert
        missingConstantFiles.Should().BeEmpty("each controller must have a dedicated route names constants file under Controllers/Constants");
        controllersUsingInlineRouteNames.Should().BeEmpty("controllers must use route name constants instead of inline literals in WithName and CreatedAtRoute");
    }

    private static string GetRepositoryRootPath()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var solutionFilePath = Path.Combine(currentDirectory.FullName, "InfraFlowSculptor.slnx");
            if (File.Exists(solutionFilePath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Could not locate the repository root from the test execution directory.");
    }

    private static string GetExpectedRouteNamesFilePath(string constantsDirectoryPath, string controllerFilePath)
    {
        var controllerName = Path.GetFileNameWithoutExtension(controllerFilePath);
        var routeNamesFileName = controllerName.Replace("Controller", string.Empty, StringComparison.Ordinal) + "RouteNames.cs";

        return Path.Combine(constantsDirectoryPath, routeNamesFileName);
    }

    private static bool ContainsInlineRouteNameLiteral(string controllerFilePath)
    {
        var fileContent = File.ReadAllText(controllerFilePath);

        return fileContent.Contains(".WithName(\"", StringComparison.Ordinal)
            || fileContent.Contains("routeName: \"", StringComparison.Ordinal);
    }
}