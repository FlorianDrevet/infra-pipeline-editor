using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.DocumentIntelligenceAggregate;

public sealed class DocumentIntelligenceTests
{
    private const string DefaultName = "doc-intel-prod";
    private const string DefaultSubDomain = "my-docintel";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const Location.LocationEnum DefaultLocation = Location.LocationEnum.FranceCentral;

    private static DocumentIntelligence CreateValid(bool isExisting = false)
        => DocumentIntelligence.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocation),
            customSubDomainName: DefaultSubDomain,
            isExisting: isExisting);

    // ─── Factory ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();

        // Act
        var sut = DocumentIntelligence.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocation),
            customSubDomainName: DefaultSubDomain);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocation);
        sut.CustomSubDomainName.Should().Be(DefaultSubDomain);
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullSubDomain_When_Create_Then_SubDomainIsNull()
    {
        // Act
        var sut = DocumentIntelligence.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocation),
            customSubDomainName: null);

        // Assert
        sut.CustomSubDomainName.Should().BeNull();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PopulatesCollection()
    {
        // Arrange
        var settings = new (string, DocumentIntelligenceSku?, PublicNetworkAccessMode?, bool)[]
        {
            (DevEnvironment, new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.F0), new PublicNetworkAccessMode(PublicNetworkAccessMode.Mode.Enabled), false),
            (ProdEnvironment, new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.S0), new PublicNetworkAccessMode(PublicNetworkAccessMode.Mode.Disabled), true),
        };

        // Act
        var sut = DocumentIntelligence.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocation),
            customSubDomainName: null,
            environmentSettings: settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_IgnoresEnvironmentSettings()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (DocumentIntelligenceSku?)new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.F0), (PublicNetworkAccessMode?)null, false),
        };

        // Act
        var sut = DocumentIntelligence.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocation),
            customSubDomainName: DefaultSubDomain,
            environmentSettings: settings,
            isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewValues_When_Update_Then_SetsNameLocationAndSubDomain()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        sut.Update(
            new Name("doc-intel-renamed"),
            new Location(Location.LocationEnum.WestEurope),
            "new-subdomain");

        // Assert
        sut.Name.Value.Should().Be("doc-intel-renamed");
        sut.Location.Value.Should().Be(Location.LocationEnum.WestEurope);
        sut.CustomSubDomainName.Should().Be("new-subdomain");
    }

    [Fact]
    public void Given_IsExisting_When_Update_Then_DoesNotChangeSubDomain()
    {
        // Arrange
        var sut = CreateValid(isExisting: true);
        var originalSubDomain = sut.CustomSubDomainName;

        // Act
        sut.Update(
            new Name("doc-intel-renamed"),
            new Location(Location.LocationEnum.WestEurope),
            "should-be-ignored");

        // Assert — IsExisting guard: subdomain must not change
        sut.CustomSubDomainName.Should().Be(originalSubDomain);
        // Name and Location are always updated
        sut.Name.Value.Should().Be("doc-intel-renamed");
    }

    // ─── SetEnvironmentSettings ───────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        sut.SetEnvironmentSettings(
            ProdEnvironment,
            new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.S0),
            new PublicNetworkAccessMode(PublicNetworkAccessMode.Mode.Disabled),
            disableLocalAuth: true);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
        sut.EnvironmentSettings.Single().DisableLocalAuth.Should().BeTrue();
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntry()
    {
        // Arrange
        var sut = CreateValid();
        sut.SetEnvironmentSettings(ProdEnvironment, new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.F0), null, false);

        // Act
        sut.SetEnvironmentSettings(
            ProdEnvironment,
            new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.S0),
            new PublicNetworkAccessMode(PublicNetworkAccessMode.Mode.Disabled),
            disableLocalAuth: true);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().Sku!.Value.Should().Be(DocumentIntelligenceSku.Sku.S0);
        sut.EnvironmentSettings.Single().DisableLocalAuth.Should().BeTrue();
    }

    [Fact]
    public void Given_IsExisting_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValid(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(
            ProdEnvironment,
            new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.S0),
            null,
            disableLocalAuth: true);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── SetAllEnvironmentSettings ────────────────────────────────────────────

    [Fact]
    public void Given_MultipleSettings_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValid();
        sut.SetEnvironmentSettings(DevEnvironment, null, null, false);

        var newSettings = new[]
        {
            (ProdEnvironment, (DocumentIntelligenceSku?)new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.S0), (PublicNetworkAccessMode?)null, true),
        };

        // Act
        sut.SetAllEnvironmentSettings(newSettings);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
    }

    [Fact]
    public void Given_IsExisting_When_SetAllEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValid(isExisting: true);
        var newSettings = new[]
        {
            (DevEnvironment, (DocumentIntelligenceSku?)null, (PublicNetworkAccessMode?)null, false),
        };

        // Act
        sut.SetAllEnvironmentSettings(newSettings);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }
}
