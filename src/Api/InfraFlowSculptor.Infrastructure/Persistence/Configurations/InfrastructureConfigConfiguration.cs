using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Extensions;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public sealed class InfrastructureConfigConfiguration
    : IEntityTypeConfiguration<InfrastructureConfig>
{
    public void Configure(EntityTypeBuilder<InfrastructureConfig> builder)
    {
        builder.ToTable("InfrastructureConfigs");

        builder.HasKey(x => x.Id);

        builder.ConfigureAggregateRootId<InfrastructureConfig, InfrastructureConfigId>();

        builder.Property(x => x.Name)
            .HasConversion(new SingleValueConverter<Name, string>());

        builder.Property(x => x.DefaultNamingTemplate)
#pragma warning disable CS8620 // Nullability mismatch — EF Core handles null conversion internally
            .HasConversion(new SingleValueConverter<NamingTemplate, string>())
#pragma warning restore CS8620
            .IsRequired(false);

        // ========================
        // ProjectId (required FK)
        // ========================
        builder.Property(x => x.ProjectId)
            .HasConversion(new IdValueConverter<ProjectId>())
            .IsRequired();

        builder.HasIndex(x => x.ProjectId);

        builder.HasOne<Domain.ProjectAggregate.Project>()
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ========================
        // Inheritance flags
        // ========================
        builder.Property(x => x.UseProjectNamingConventions)
            .HasDefaultValue(true);

        builder.Property(x => x.AppPipelineMode)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(AppPipelineMode.Isolated);

        builder.Property(x => x.AgentPoolName)
            .HasMaxLength(200)
            .IsRequired(false);

        // ========================
        // ResourceGroups (Entity)
        // ========================
        builder.HasMany(x => x.ResourceGroups)
            .WithOne(x => x.InfraConfig)
            .HasForeignKey(x => x.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        // ========================
        // ParameterDefinitions (Entity)
        // ========================
        builder.HasMany(x => x.ParameterDefinitions)
            .WithOne()
            .HasForeignKey(x => x.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        // ========================
        // ResourceNamingTemplates (Entity)
        // ========================
        builder.HasMany(x => x.ResourceNamingTemplates)
            .WithOne(x => x.InfraConfig)
            .HasForeignKey(x => x.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        // ========================
        // CrossConfigResourceReferences (Entity)
        // ========================
        builder.HasMany(x => x.CrossConfigReferences)
            .WithOne()
            .HasForeignKey(x => x.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        // ========================
        // Tags (OWNED)
        // ========================
        builder.OwnsMany(c => c.Tags, tag =>
        {
            tag.ToTable("InfrastructureConfigTags");
            tag.WithOwner().HasForeignKey("InfrastructureConfigId");
            tag.HasKey("InfrastructureConfigId", "Name");
            tag.Property(t => t.Name).HasMaxLength(100);
            tag.Property(t => t.Value).HasMaxLength(500);
        });
        builder.Navigation(c => c.Tags).HasField("_tags").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}