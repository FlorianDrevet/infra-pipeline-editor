using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public sealed class ResourceNamingTemplateConfiguration
    : IEntityTypeConfiguration<ResourceNamingTemplate>
{
    public void Configure(EntityTypeBuilder<ResourceNamingTemplate> builder)
    {
        builder.ToTable("ResourceNamingTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ResourceNamingTemplateId>())
            .ValueGeneratedNever();

        builder.Property(x => x.InfraConfigId)
            .HasConversion(new IdValueConverter<InfrastructureConfigId>());

        builder.Property(x => x.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Template)
            .IsRequired()
            .HasConversion(new SingleValueConverter<NamingTemplate, string>());

        builder.HasIndex(x => new { x.InfraConfigId, x.ResourceType })
            .IsUnique();
    }
}