using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="ProjectResourceAbbreviation"/>.</summary>
public sealed class ProjectResourceAbbreviationConfiguration
    : IEntityTypeConfiguration<ProjectResourceAbbreviation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectResourceAbbreviation> builder)
    {
        builder.ToTable("ProjectResourceAbbreviations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ProjectResourceAbbreviationId>())
            .ValueGeneratedNever();

        builder.Property(x => x.ProjectId)
            .HasConversion(new IdValueConverter<ProjectId>());

        builder.Property(x => x.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Abbreviation)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => new { x.ProjectId, x.ResourceType })
            .IsUnique();
    }
}
