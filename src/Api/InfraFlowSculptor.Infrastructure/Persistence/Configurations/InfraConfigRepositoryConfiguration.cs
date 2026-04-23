using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="InfraConfigRepository"/> child entity.</summary>
public sealed class InfraConfigRepositoryConfiguration : IEntityTypeConfiguration<InfraConfigRepository>
{
    private const string TableName = "InfraConfigRepositories";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InfraConfigRepository> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<InfraConfigRepositoryId>())
            .ValueGeneratedNever();

        builder.Property(x => x.InfrastructureConfigId)
            .HasConversion(new IdValueConverter<InfrastructureConfigId>())
            .IsRequired();

        builder.Property(x => x.Alias)
            .HasConversion(new RepositoryAliasConverter())
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderType)
            .HasConversion(new EnumValueConverter<GitProviderType, GitProviderTypeEnum>())
            .IsRequired();

        builder.Property(x => x.RepositoryUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Owner)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.RepositoryName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DefaultBranch)
            .IsRequired()
            .HasMaxLength(200)
            .HasDefaultValue("main");

        builder.Property(x => x.ContentKinds)
            .HasConversion(new RepositoryContentKindsConverter())
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => new { x.InfrastructureConfigId, x.Alias })
            .IsUnique();
    }
}
