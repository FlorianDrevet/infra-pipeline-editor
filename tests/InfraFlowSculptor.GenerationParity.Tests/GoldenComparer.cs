using System.Runtime.CompilerServices;

namespace InfraFlowSculptor.GenerationParity.Tests;

/// <summary>
/// Loads / writes golden files and compares engine output against them.
/// When compiled with <c>-p:DefineConstants=REGENERATE_GOLDENS</c>, writes the
/// current engine output into the source tree instead of asserting.
/// </summary>
internal static class GoldenComparer
{
    /// <summary>
    /// Absolute path of the test project folder (resolved at compile time
    /// so regeneration writes to the source tree, not to bin/).
    /// </summary>
    private static string ProjectDir([CallerFilePath] string sourceFilePath = "")
        => Path.GetDirectoryName(sourceFilePath)!;

    /// <summary>
    /// Compares <paramref name="actualFiles"/> against the golden folder
    /// <c>Fixtures/{fixtureName}/golden/</c>. Each file is compared individually
    /// so the xUnit diff report points at the exact divergent file.
    /// Newlines are normalized to LF to avoid CRLF/LF churn.
    /// </summary>
    public static void AssertMatchesGolden(
        string fixtureName,
        IReadOnlyDictionary<string, string> actualFiles)
    {
        var projectDir = ProjectDir();
        var goldenDir = Path.Combine(projectDir, "Fixtures", fixtureName, "golden");

#if REGENERATE_GOLDENS
        RegenerateGolden(goldenDir, actualFiles);
        return;
#else
        if (!Directory.Exists(goldenDir))
        {
            throw new DirectoryNotFoundException(
                $"Golden folder '{goldenDir}' not found. Regenerate with -p:DefineConstants=REGENERATE_GOLDENS.");
        }

        var actualNormalized = actualFiles
            .ToDictionary(kv => kv.Key.Replace('\\', '/'), kv => Normalize(kv.Value));

        var goldenFiles = Directory.EnumerateFiles(goldenDir, "*", SearchOption.AllDirectories)
            .ToDictionary(
                f => Path.GetRelativePath(goldenDir, f).Replace('\\', '/'),
                f => Normalize(File.ReadAllText(f)));

        // 1) Path-level parity: fail fast with a clear message if the set of files diverges.
        var missingInActual = goldenFiles.Keys.Except(actualNormalized.Keys).OrderBy(x => x).ToList();
        var unexpectedInActual = actualNormalized.Keys.Except(goldenFiles.Keys).OrderBy(x => x).ToList();
        if (missingInActual.Count > 0 || unexpectedInActual.Count > 0)
        {
            var msg = new System.Text.StringBuilder();
            msg.AppendLine($"Fixture '{fixtureName}' file-set mismatch.");
            if (missingInActual.Count > 0)
            {
                msg.AppendLine("Missing in engine output (present in golden):");
                foreach (var p in missingInActual) msg.AppendLine($"  - {p}");
            }
            if (unexpectedInActual.Count > 0)
            {
                msg.AppendLine("Unexpected in engine output (absent from golden):");
                foreach (var p in unexpectedInActual) msg.AppendLine($"  + {p}");
            }
            Xunit.Assert.Fail(msg.ToString());
        }

        // 2) Content parity per file — sorted so diff is deterministic.
        foreach (var path in goldenFiles.Keys.OrderBy(p => p, StringComparer.Ordinal))
        {
            var expected = goldenFiles[path];
            var actual = actualNormalized[path];
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                Xunit.Assert.Fail(BuildFirstDiffMessage(fixtureName, path, expected, actual));
            }
        }
#endif
    }

#if REGENERATE_GOLDENS
    private static void RegenerateGolden(string goldenDir, IReadOnlyDictionary<string, string> actualFiles)
    {
        if (Directory.Exists(goldenDir))
        {
            Directory.Delete(goldenDir, recursive: true);
        }
        Directory.CreateDirectory(goldenDir);

        foreach (var (relPath, content) in actualFiles)
        {
            var target = Path.Combine(goldenDir, relPath.Replace('/', Path.DirectorySeparatorChar));
            var dir = Path.GetDirectoryName(target);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(target, Normalize(content));
        }
    }
#endif

    private static string Normalize(string text)
        => text.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static string BuildFirstDiffMessage(string fixtureName, string relativePath, string expected, string actual)
    {
        var expectedLines = expected.Split('\n');
        var actualLines = actual.Split('\n');
        var max = Math.Max(expectedLines.Length, actualLines.Length);
        var firstDiff = -1;
        for (var i = 0; i < max; i++)
        {
            var e = i < expectedLines.Length ? expectedLines[i] : "<EOF>";
            var a = i < actualLines.Length ? actualLines[i] : "<EOF>";
            if (!string.Equals(e, a, StringComparison.Ordinal))
            {
                firstDiff = i;
                break;
            }
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Fixture '{fixtureName}' content mismatch on file: {relativePath}");
        sb.AppendLine($"First divergent line (0-based): {firstDiff}");
        sb.AppendLine("── Expected (golden) ──");
        for (var i = firstDiff; i < Math.Min(firstDiff + 5, expectedLines.Length); i++)
            sb.AppendLine($"  {i,4} | {expectedLines[i]}");
        sb.AppendLine("── Actual (engine) ──");
        for (var i = firstDiff; i < Math.Min(firstDiff + 5, actualLines.Length); i++)
            sb.AppendLine($"  {i,4} | {actualLines[i]}");
        return sb.ToString();
    }

    /// <summary>
    /// Flattens a mono-repo result (CommonFiles + per-config ConfigFiles) into a single
    /// path → content dictionary: <c>common/...</c> and <c>{configName}/...</c>.
    /// </summary>
    public static IReadOnlyDictionary<string, string> FlattenMonoRepo(
        IReadOnlyDictionary<string, string> commonFiles,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> configFiles)
    {
        var flat = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (path, content) in commonFiles)
        {
            flat[$"common/{path.Replace('\\', '/')}"] = content;
        }
        foreach (var (configName, files) in configFiles)
        {
            foreach (var (path, content) in files)
            {
                flat[$"{configName}/{path.Replace('\\', '/')}"] = content;
            }
        }
        return flat;
    }
}
