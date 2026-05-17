using FluentAssertions;

namespace InfraFlowSculptor.GenerationCore.Tests;

public sealed class PathSanitizerTests
{
    [Fact]
    public void Sanitize_GivenNull_ShouldReturnNull()
    {
        PathSanitizer.Sanitize(null!).Should().BeNull();
    }

    [Fact]
    public void Sanitize_GivenEmptyString_ShouldReturnEmptyString()
    {
        PathSanitizer.Sanitize("").Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_GivenWhitespaceOnly_ShouldReturnWhitespace()
    {
        PathSanitizer.Sanitize("   ").Should().Be("   ");
    }

    [Fact]
    public void Sanitize_GivenSimpleName_ShouldReturnUnchanged()
    {
        PathSanitizer.Sanitize("simple").Should().Be("simple");
    }

    [Fact]
    public void Sanitize_GivenSpaces_ShouldReplaceDashes()
    {
        PathSanitizer.Sanitize("hello world").Should().Be("hello-world");
    }

    [Fact]
    public void Sanitize_GivenUnderscores_ShouldReplaceDashes()
    {
        PathSanitizer.Sanitize("hello_world").Should().Be("hello-world");
    }

    [Fact]
    public void Sanitize_GivenMultipleUnderscores_ShouldCollapseToDash()
    {
        PathSanitizer.Sanitize("hello__world").Should().Be("hello-world");
    }

    [Fact]
    public void Sanitize_GivenMultipleSpaces_ShouldCollapseToDash()
    {
        PathSanitizer.Sanitize("hello   world").Should().Be("hello-world");
    }

    [Fact]
    public void Sanitize_GivenConsecutiveDashes_ShouldCollapseToDash()
    {
        PathSanitizer.Sanitize("hello---world").Should().Be("hello-world");
    }

    [Fact]
    public void Sanitize_GivenLeadingDash_ShouldTrimLeadingDash()
    {
        PathSanitizer.Sanitize("-leading").Should().Be("leading");
    }

    [Fact]
    public void Sanitize_GivenTrailingDash_ShouldTrimTrailingDash()
    {
        PathSanitizer.Sanitize("trailing-").Should().Be("trailing");
    }

    [Fact]
    public void Sanitize_GivenAtSign_ShouldRemoveInvalidChar()
    {
        // @ is not in [\w\-.], so it gets removed by InvalidPathChars regex
        PathSanitizer.Sanitize("hello@world").Should().Be("helloworld");
    }

    [Fact]
    public void Sanitize_GivenDots_ShouldPreserveDots()
    {
        PathSanitizer.Sanitize("hello.world").Should().Be("hello.world");
    }

    [Fact]
    public void Sanitize_GivenComplexInput_ShouldSanitizeCorrectly()
    {
        // "  My Config_Name--test  "
        // Spaces/underscores → dashes: "--My-Config-Name--test--"
        // Invalid chars removed: (none here beyond what was replaced)
        // Consecutive dashes collapsed: "-My-Config-Name-test-"
        // Trim leading/trailing dashes: "My-Config-Name-test"
        PathSanitizer.Sanitize("  My Config_Name--test  ").Should().Be("My-Config-Name-test");
    }

    [Fact]
    public void Sanitize_GivenMixedSpacesAndUnderscores_ShouldCollapseToDash()
    {
        PathSanitizer.Sanitize("a _ b").Should().Be("a-b");
    }

    [Fact]
    public void Sanitize_GivenOnlyDashes_ShouldReturnEmpty()
    {
        PathSanitizer.Sanitize("---").Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_GivenOnlyInvalidChars_ShouldReturnEmpty()
    {
        PathSanitizer.Sanitize("@#$%").Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_GivenDigitsAndLetters_ShouldReturnUnchanged()
    {
        PathSanitizer.Sanitize("abc123").Should().Be("abc123");
    }

    [Fact]
    public void Sanitize_GivenTabCharacter_ShouldReplaceDash()
    {
        PathSanitizer.Sanitize("hello\tworld").Should().Be("hello-world");
    }
}
