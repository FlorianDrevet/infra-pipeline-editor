using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="ProjectResourceNamingTemplate"/>.</summary>
public sealed class ProjectResourceNamingTemplateConfiguration
    : IEntityTypeConfiguration<ProjectResourceNamingTemplate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectResourceNamingTemplate> builder)
    {
        builder.ToTable("ProjectResourceNamingTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ProjectResourceNamingTemplateId>())
            .ValueGeneratedNever();

        builder.Property(x => x.ProjectId)
            .HasConversion(new IdValueConverter<ProjectId>());

        builder.Property(x => x.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Template)
            .IsRequired()
            .HasConversion(new SingleValueConverter<NamingTemplate, string>());

        builder.HasIndex(x => new { x.ProjectId, x.ResourceType })
            .IsUnique();
    }
}