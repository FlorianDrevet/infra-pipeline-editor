namespace InfraFlowSculptor.Api.Common;

/// <summary>
/// Preserves the historical API helper surface while delegating to the shared contracts implementation.
/// </summary>
public static class SafeRelativePath
{
    /// <summary>
    /// Attempts to normalize a user-supplied relative path.
    /// </summary>
    /// <param name="input">The raw user-supplied path.</param>
    /// <param name="normalized">The normalized path using forward slashes when valid; otherwise <see cref="string.Empty"/>.</param>
    /// <returns><c>true</c> when the input is a safe relative path; otherwise <c>false</c>.</returns>
    public static bool TryNormalize(string? input, out string normalized) =>
        InfraFlowSculptor.Contracts.Common.SafeRelativePath.TryNormalize(input, out normalized);
}
