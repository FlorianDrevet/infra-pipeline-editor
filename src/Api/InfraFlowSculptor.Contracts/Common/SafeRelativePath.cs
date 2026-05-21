namespace InfraFlowSculptor.Contracts.Common;

/// <summary>
/// Validates and normalizes user-supplied relative file paths to prevent path traversal attacks.
/// </summary>
public static class SafeRelativePath
{
    private static readonly char[] InvalidSegmentChars = ['\0'];

    /// <summary>
    /// Attempts to normalize a user-supplied relative path by rejecting traversal segments,
    /// rooted paths, drive letters, leading separators, empty segments, and null characters.
    /// </summary>
    /// <param name="input">The raw user-supplied path.</param>
    /// <param name="normalized">The normalized path using forward slashes when valid; otherwise <see cref="string.Empty"/>.</param>
    /// <returns><c>true</c> when the input is a safe relative path; otherwise <c>false</c>.</returns>
    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        if (input.IndexOfAny(InvalidSegmentChars) >= 0)
            return false;

        if (Path.IsPathRooted(input))
            return false;

        if (input.Length >= 2 && input[1] == ':')
            return false;

        if (input[0] == '/' || input[0] == '\\')
            return false;

        var segments = input.Replace('\\', '/').Split('/');
        foreach (var segment in segments)
        {
            if (string.IsNullOrEmpty(segment))
                return false;

            if (segment == "." || segment == "..")
                return false;
        }

        normalized = string.Join('/', segments);
        return true;
    }
}