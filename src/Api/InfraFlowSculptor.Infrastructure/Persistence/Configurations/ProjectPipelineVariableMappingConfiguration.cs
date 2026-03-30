using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="ProjectPipelineVariableMapping"/> entity.</summary>
public sealed class ProjectPipelineVariableMappingConfiguration
    : IEntityTypeConfiguration<ProjectPipelineVariableMapping>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectPipelineVariableMapping> builder)
    {
        builder.ToTable("ProjectPipelineVariableMappings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ProjectPipelineVariableMappingId>())
            .ValueGeneratedNever();

        builder.Property(x => x.VariableGroupId)
            .HasConversion(new IdValueConverter<ProjectPipelineVariableGroupId>())
            .IsRequired();

        builder.Property(x => x.PipelineVariableName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.BicepParameterName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => new { x.VariableGroupId, x.BicepParameterName })
            .IsUnique();
    }
}
