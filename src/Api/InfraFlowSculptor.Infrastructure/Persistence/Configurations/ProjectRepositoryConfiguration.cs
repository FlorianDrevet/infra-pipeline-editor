using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="ProjectRepository"/> child entity.</summary>
public sealed class ProjectRepositoryConfiguration : IEntityTypeConfiguration<ProjectRepository>
{
    private const string TableName = "ProjectRepositories";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectRepository> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ProjectRepositoryId>())
            .ValueGeneratedNever();

        builder.Property(x => x.ProjectId)
            .HasConversion(new IdValueConverter<ProjectId>())
            .IsRequired();

        builder.Property(x => x.Alias)
            .HasConversion(new RepositoryAliasConverter())
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderType)
            .HasConversion(new EnumValueConverter<GitProviderType, GitProviderTypeEnum>())
            .IsRequired(false);

        builder.Property(x => x.RepositoryUrl)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.Owner)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(x => x.RepositoryName)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(x => x.DefaultBranch)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(x => x.ContentKinds)
            .HasConversion(new RepositoryContentKindsConverter())
            .HasMaxLength(100)
            .IsRequired();

        // Unique alias per project
        builder.HasIndex(x => new { x.ProjectId, x.Alias })
            .IsUnique();
    }
}
