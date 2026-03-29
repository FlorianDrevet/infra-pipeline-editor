using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="GitRepositoryConfiguration"/> entity.</summary>
public sealed class GitRepositoryConfigurationConfiguration
    : IEntityTypeConfiguration<GitRepositoryConfiguration>
{
    private const string TableName = "GitRepositoryConfigurations";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GitRepositoryConfiguration> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<GitRepositoryConfigurationId>())
            .ValueGeneratedNever();

        builder.Property(x => x.ProviderType)
            .HasConversion(new EnumValueConverter<GitProviderType, GitProviderTypeEnum>())
            .IsRequired();

        builder.Property(x => x.RepositoryUrl).IsRequired().HasMaxLength(500);
        builder.Property(x => x.DefaultBranch).IsRequired().HasMaxLength(200).HasDefaultValue("main");
        builder.Property(x => x.BasePath).HasMaxLength(500);
        builder.Property(x => x.PipelineBasePath).HasMaxLength(500);
        builder.Property(x => x.Owner).IsRequired().HasMaxLength(200);
        builder.Property(x => x.RepositoryName).IsRequired().HasMaxLength(200);

        builder.Property(x => x.ProjectId)
            .HasConversion(new IdValueConverter<ProjectId>());

        builder.HasIndex(x => x.ProjectId).IsUnique();
    }
}
