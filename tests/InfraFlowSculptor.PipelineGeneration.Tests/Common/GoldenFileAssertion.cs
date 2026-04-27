using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Common;

/// <summary>
/// Assertion helper for byte-for-byte parity tests against versioned golden files.
/// </summary>
/// <remarks>
/// Set the environment variable <c>IFS_UPDATE_GOLDEN=true</c> to (re)capture goldens
/// from the current engine output. The helper writes back to the project source folder
/// (not <c>bin/</c>) so the captured files can be committed.
/// </remarks>
internal static class GoldenFileAssertion
{
    private const string ProjectMarker = "InfraFlowSculptor.PipelineGeneration.Tests.csproj";
    private const string UpdateEnvironmentVariable = "IFS_UPDATE_GOLDEN";

    /// <summary>
    /// Asserts that <paramref name="actual"/> matches the golden file located at
    /// <c>GoldenFiles/{goldenRelativePath}</c> in the test project source folder.
    /// </summary>
    /// <param name="actual">The actual content produced by the engine under test.</param>
    /// <param name="goldenRelativePath">Path relative to <c>GoldenFiles/</c>, using forward slashes.</param>
    public static void AssertMatches(string actual, string goldenRelativePath)
    {
        var normalizedRelative = goldenRelativePath.Replace('\\', Path.DirectorySeparatorChar)
                                                   .Replace('/', Path.DirectorySeparatorChar);

        var projectDir = ResolveProjectDirectory();
        var sourcePath = Path.Combine(projectDir, "GoldenFiles", normalizedRelative);

        if (string.Equals(System.Environment.GetEnvironmentVariable(UpdateEnvironmentVariable), "true",
                System.StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            File.WriteAllText(sourcePath, actual);
            return;
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(
                $"Golden file not found: {sourcePath}. " +
                $"Run with environment variable {UpdateEnvironmentVariable}=true to capture it.",
                sourcePath);
        }

        var expected = File.ReadAllText(sourcePath);
        var normalizedExpected = expected.Replace("\r\n", "\n");
        var normalizedActual = actual.Replace("\r\n", "\n");

        try
        {
            normalizedActual.Should().Be(normalizedExpected,
                "golden file '{0}' should match the engine output", goldenRelativePath);
        }
        catch
        {
            DumpActual(actual, normalizedRelative);
            throw;
        }
    }

    /// <summary>
    /// Asserts that every entry in <paramref name="actualFiles"/> matches its corresponding
    /// golden file under <c>GoldenFiles/{goldenSubFolder}/</c>, and that the set of keys is
    /// exactly the set of files present in that folder.
    /// </summary>
    public static void AssertDictionaryMatches(
        IReadOnlyDictionary<string, string> actualFiles,
        string goldenSubFolder)
    {
        var normalizedSubFolder = goldenSubFolder.Replace('\\', '/');

        foreach (var (relativePath, content) in actualFiles)
        {
            AssertMatches(content, $"{normalizedSubFolder}/{relativePath}");
        }

        // Skip the file-set check during capture mode: the folder is being populated.
        if (string.Equals(System.Environment.GetEnvironmentVariable(UpdateEnvironmentVariable), "true",
                System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var projectDir = ResolveProjectDirectory();
        var folderRoot = Path.Combine(projectDir, "GoldenFiles",
            normalizedSubFolder.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(folderRoot))
        {
            // No folder means no expected files — only acceptable if dictionary is empty.
            actualFiles.Should().BeEmpty(
                "no golden folder exists at '{0}', so the engine should produce no files", folderRoot);
            return;
        }

        var expectedKeys = Directory
            .EnumerateFiles(folderRoot, "*", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(folderRoot, p).Replace('\\', '/'))
            .ToHashSet(System.StringComparer.Ordinal);

        var actualKeys = actualFiles.Keys
            .Select(k => k.Replace('\\', '/'))
            .ToHashSet(System.StringComparer.Ordinal);

        actualKeys.Should().BeEquivalentTo(expectedKeys,
            "the set of generated files should exactly match the golden folder '{0}'", goldenSubFolder);
    }

    private static string ResolveProjectDirectory()
    {
        var current = new DirectoryInfo(System.AppContext.BaseDirectory);
        while (current is not null)
        {
            if (current.GetFiles(ProjectMarker, SearchOption.TopDirectoryOnly).Length > 0)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate test project directory containing '{ProjectMarker}' from '{System.AppContext.BaseDirectory}'.");
    }

    private static void DumpActual(string actual, string normalizedRelative)
    {
        var dumpPath = Path.Combine(System.AppContext.BaseDirectory, "GoldenFiles", "__actual__", normalizedRelative);
        Directory.CreateDirectory(Path.GetDirectoryName(dumpPath)!);
        File.WriteAllText(dumpPath, actual);
    }
}
