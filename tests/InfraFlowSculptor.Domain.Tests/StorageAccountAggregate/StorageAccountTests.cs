using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.StorageAccountAggregate;

public sealed class StorageAccountTests
{
    private const string DefaultStorageName = "stproddata";
    private const string ContainerName = "uploads";
    private const string DuplicateContainerName = "UPLOADS";
    private const string QueueName = "jobs";
    private const string TableName = "events";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static StorageAccount CreateValidStorageAccount(bool isExisting = false)
    {
        return StorageAccount.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultStorageName),
            new Location(DefaultLocationValue),
            new StorageAccountKind(StorageAccountKind.Kind.StorageV2),
            new StorageAccessTier(StorageAccessTier.Tier.Hot),
            allowBlobPublicAccess: false,
            enableHttpsTrafficOnly: true,
            new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls12),
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Act
        var sut = CreateValidStorageAccount();

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.Name.Value.Should().Be(DefaultStorageName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.Kind.Value.Should().Be(StorageAccountKind.Kind.StorageV2);
        sut.AccessTier.Value.Should().Be(StorageAccessTier.Tier.Hot);
        sut.AllowBlobPublicAccess.Should().BeFalse();
        sut.EnableHttpsTrafficOnly.Should().BeTrue();
        sut.MinimumTlsVersion.Value.Should().Be(StorageAccountTlsVersion.Version.Tls12);
        sut.IsExisting.Should().BeFalse();
        sut.BlobContainers.Should().BeEmpty();
        sut.Queues.Should().BeEmpty();
        sut.Tables.Should().BeEmpty();
        sut.AllCorsRules.Should().BeEmpty();
        sut.LifecycleRules.Should().BeEmpty();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_SkipsOptionalCollections()
    {
        // Arrange
        var environmentSettings = new[] { ("dev", (StorageAccountSku?)new StorageAccountSku(StorageAccountSku.Sku.Standard_LRS)) };

        // Act
        var sut = StorageAccount.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultStorageName),
            new Location(DefaultLocationValue),
            new StorageAccountKind(StorageAccountKind.Kind.StorageV2),
            new StorageAccessTier(StorageAccessTier.Tier.Hot),
            allowBlobPublicAccess: false,
            enableHttpsTrafficOnly: true,
            new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls12),
            environmentSettings: environmentSettings,
            isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Blob Containers ────────────────────────────────────────────────────

    [Fact]
    public void Given_NewName_When_AddBlobContainer_Then_AddsContainer()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        var publicAccess = new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.None);

        // Act
        var result = sut.AddBlobContainer(ContainerName, publicAccess);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(ContainerName);
        result.Value.PublicAccess.Should().Be(publicAccess);
        sut.BlobContainers.Should().ContainSingle();
    }

    [Fact]
    public void Given_DuplicateNameCaseInsensitive_When_AddBlobContainer_Then_ReturnsConflictError()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        var publicAccess = new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.None);
        sut.AddBlobContainer(ContainerName, publicAccess);

        // Act
        var result = sut.AddBlobContainer(DuplicateContainerName, publicAccess);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("StorageAccount.DuplicateBlobContainerName");
        sut.BlobContainers.Should().ContainSingle();
    }

    [Fact]
    public void Given_ExistingContainer_When_UpdateBlobContainerPublicAccess_Then_UpdatesAccess()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        var addResult = sut.AddBlobContainer(
            ContainerName,
            new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.None));
        var newAccess = new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.Blob);

        // Act
        var result = sut.UpdateBlobContainerPublicAccess(addResult.Value.Id, newAccess);

        // Assert
        result.IsError.Should().BeFalse();
        sut.BlobContainers.Single().PublicAccess.Should().Be(newAccess);
    }

    [Fact]
    public void Given_UnknownContainerId_When_UpdateBlobContainerPublicAccess_Then_ReturnsNotFoundError()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        var unknownId = BlobContainerId.CreateUnique();

        // Act
        var result = sut.UpdateBlobContainerPublicAccess(
            unknownId,
            new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.None));

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("StorageAccount.BlobContainerNotFound");
    }

    // ─── Queues ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewName_When_AddQueue_Then_AddsQueue()
    {
        // Arrange
        var sut = CreateValidStorageAccount();

        // Act
        var result = sut.AddQueue(QueueName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(QueueName);
        sut.Queues.Should().ContainSingle();
    }

    [Fact]
    public void Given_DuplicateQueueNameCaseInsensitive_When_AddQueue_Then_ReturnsConflictError()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        sut.AddQueue(QueueName);

        // Act
        var result = sut.AddQueue(QueueName.ToUpperInvariant());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("StorageAccount.DuplicateQueueName");
        sut.Queues.Should().ContainSingle();
    }

    // ─── Tables ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewName_When_AddTable_Then_AddsTable()
    {
        // Arrange
        var sut = CreateValidStorageAccount();

        // Act
        var result = sut.AddTable(TableName);

        // Assert
        result.IsError.Should().BeFalse();
        sut.Tables.Should().ContainSingle();
    }

    [Fact]
    public void Given_DuplicateTableNameCaseInsensitive_When_AddTable_Then_ReturnsConflictError()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        sut.AddTable(TableName);

        // Act
        var result = sut.AddTable(TableName.ToUpperInvariant());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("StorageAccount.DuplicateTableName");
    }

    // ─── Environment settings ───────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        var sku = new StorageAccountSku(StorageAccountSku.Sku.Standard_LRS);

        // Act
        sut.SetEnvironmentSettings("dev", sku);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().EnvironmentName.Should().Be("dev");
        sut.EnvironmentSettings.Single().Sku.Should().Be(sku);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        sut.SetEnvironmentSettings("dev", new StorageAccountSku(StorageAccountSku.Sku.Standard_LRS));
        var newSku = new StorageAccountSku(StorageAccountSku.Sku.Premium_LRS);

        // Act
        sut.SetEnvironmentSettings("dev", newSku);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().Sku.Should().Be(newSku);
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidStorageAccount(isExisting: true);

        // Act
        sut.SetEnvironmentSettings("dev", new StorageAccountSku(StorageAccountSku.Sku.Standard_LRS));

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        sut.SetEnvironmentSettings("dev", new StorageAccountSku(StorageAccountSku.Sku.Standard_LRS));
        var settings = new[]
        {
            ("staging", (StorageAccountSku?)new StorageAccountSku(StorageAccountSku.Sku.Standard_GRS)),
            ("prod", (StorageAccountSku?)new StorageAccountSku(StorageAccountSku.Sku.Premium_LRS)),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
        sut.EnvironmentSettings.Should().NotContain(es => es.EnvironmentName == "dev");
    }

    // ─── CORS Rules ─────────────────────────────────────────────────────────

    [Fact]
    public void Given_BlobCorsRules_When_SetCorsRules_Then_OnlyBlobRulesAreReplaced()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        sut.SetTableCorsRules(new[]
        {
            ((IReadOnlyList<string>)new[] { "https://table.example.com" },
             (IReadOnlyList<string>)new[] { "GET" },
             (IReadOnlyList<string>)Array.Empty<string>(),
             (IReadOnlyList<string>)Array.Empty<string>(),
             3600),
        });
        var blobRules = new[]
        {
            ((IReadOnlyList<string>)new[] { "https://blob.example.com" },
             (IReadOnlyList<string>)new[] { "GET", "POST" },
             (IReadOnlyList<string>)Array.Empty<string>(),
             (IReadOnlyList<string>)Array.Empty<string>(),
             1800),
        };

        // Act
        sut.SetCorsRules(blobRules);

        // Assert
        sut.CorsRules.Should().ContainSingle();
        sut.TableCorsRules.Should().ContainSingle();
        sut.AllCorsRules.Should().HaveCount(2);
    }

    // ─── Lifecycle Rules ────────────────────────────────────────────────────

    [Fact]
    public void Given_LifecycleRules_When_SetLifecycleRules_Then_ReplacesExistingRules()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        var initial = new[]
        {
            ("rule-old", (IReadOnlyList<string>)new[] { "containerA" }, 7),
        };
        sut.SetLifecycleRules(initial);
        var replacement = new[]
        {
            ("rule-new-a", (IReadOnlyList<string>)new[] { "containerA" }, 30),
            ("rule-new-b", (IReadOnlyList<string>)new[] { "containerB" }, 60),
        };

        // Act
        sut.SetLifecycleRules(replacement);

        // Assert
        sut.LifecycleRules.Should().HaveCount(2);
        sut.LifecycleRules.Should().NotContain(rule => rule.RuleName == "rule-old");
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NotExisting_When_Update_Then_UpdatesAllProperties()
    {
        // Arrange
        var sut = CreateValidStorageAccount();
        var newName = new Name("strenamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);
        var newKind = new StorageAccountKind(StorageAccountKind.Kind.BlobStorage);
        var newTier = new StorageAccessTier(StorageAccessTier.Tier.Cool);
        var newTls = new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls11);

        // Act
        sut.Update(newName, newLocation, newKind, newTier, true, false, newTls);

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.Kind.Should().Be(newKind);
        sut.AccessTier.Should().Be(newTier);
        sut.AllowBlobPublicAccess.Should().BeTrue();
        sut.EnableHttpsTrafficOnly.Should().BeFalse();
        sut.MinimumTlsVersion.Should().Be(newTls);
    }

    [Fact]
    public void Given_IsExistingResource_When_Update_Then_OnlyNameAndLocationChange()
    {
        // Arrange
        var sut = CreateValidStorageAccount(isExisting: true);
        var newName = new Name("strenamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);
        var newKind = new StorageAccountKind(StorageAccountKind.Kind.BlobStorage);
        var newTier = new StorageAccessTier(StorageAccessTier.Tier.Cool);
        var newTls = new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls10);

        // Act
        sut.Update(newName, newLocation, newKind, newTier, true, false, newTls);

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.Kind.Value.Should().Be(StorageAccountKind.Kind.StorageV2);
        sut.AccessTier.Value.Should().Be(StorageAccessTier.Tier.Hot);
        sut.AllowBlobPublicAccess.Should().BeFalse();
        sut.EnableHttpsTrafficOnly.Should().BeTrue();
        sut.MinimumTlsVersion.Value.Should().Be(StorageAccountTlsVersion.Version.Tls12);
    }
}
