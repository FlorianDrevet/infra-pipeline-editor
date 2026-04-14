using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="ProjectPipelineVariableGroup"/> entity.</summary>
public sealed class ProjectPipelineVariableGroupConfiguration
    : IEntityTypeConfiguration<ProjectPipelineVariableGroup>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectPipelineVariableGroup> builder)
    {
        builder.ToTable("ProjectPipelineVariableGroups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ProjectPipelineVariableGroupId>())
            .ValueGeneratedNever();

        builder.Property(x => x.ProjectId)
            .HasConversion(new IdValueConverter<ProjectId>())
            .IsRequired();

        builder.Property(x => x.GroupName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => new { x.ProjectId, x.GroupName })
            .IsUnique();
    }
}
