using FluentAssertions;

namespace InfraFlowSculptor.GenerationCore.Tests;

public sealed class KeyVaultSecretNameRulesTests
{
    [Fact]
    public void MinLength_ShouldBe1()
    {
        KeyVaultSecretNameRules.MinLength.Should().Be(1);
    }

    [Fact]
    public void MaxLength_ShouldBe127()
    {
        KeyVaultSecretNameRules.MaxLength.Should().Be(127);
    }

    [Fact]
    public void ValidationMessage_ShouldContainExpectedText()
    {
        KeyVaultSecretNameRules.ValidationMessage
            .Should().Be("Key Vault secret names must be 1 to 127 characters long and contain only letters, digits, and hyphens.");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abc-123")]
    [InlineData("A-B-C")]
    [InlineData("0123456789")]
    [InlineData("abcdefghij")]
    [InlineData("a-b-c-d")]
    public void IsValid_GivenValidSecretName_ShouldReturnTrue(string secretName)
    {
        KeyVaultSecretNameRules.IsValid(secretName).Should().BeTrue();
    }

    [Fact]
    public void IsValid_GivenExactly127Characters_ShouldReturnTrue()
    {
        var name = new string('a', 127);

        KeyVaultSecretNameRules.IsValid(name).Should().BeTrue();
    }

    [Fact]
    public void IsValid_GivenSingleCharacter_ShouldReturnTrue()
    {
        KeyVaultSecretNameRules.IsValid("x").Should().BeTrue();
    }

    [Fact]
    public void IsValid_GivenSingleDigit_ShouldReturnTrue()
    {
        KeyVaultSecretNameRules.IsValid("7").Should().BeTrue();
    }

    [Fact]
    public void IsValid_GivenSingleHyphen_ShouldReturnTrue()
    {
        KeyVaultSecretNameRules.IsValid("-").Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("name with spaces")]
    [InlineData("name_underscore")]
    [InlineData("name.dot")]
    [InlineData("name@special")]
    [InlineData("name!bang")]
    [InlineData("name/slash")]
    [InlineData("name\\backslash")]
    public void IsValid_GivenInvalidSecretName_ShouldReturnFalse(string secretName)
    {
        KeyVaultSecretNameRules.IsValid(secretName).Should().BeFalse();
    }

    [Fact]
    public void IsValid_Given128Characters_ShouldReturnFalse()
    {
        var name = new string('a', 128);

        KeyVaultSecretNameRules.IsValid(name).Should().BeFalse();
    }

    [Fact]
    public void IsValid_GivenNull_ShouldThrow()
    {
        var act = () => KeyVaultSecretNameRules.IsValid(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
