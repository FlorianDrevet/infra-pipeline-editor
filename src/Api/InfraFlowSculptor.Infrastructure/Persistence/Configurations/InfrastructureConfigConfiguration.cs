using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.EnvironmentParameterValue;
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
            .HasConversion(new SingleValueConverter<NamingTemplate, string>())
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
        builder.Property(x => x.UseProjectEnvironments)
            .HasDefaultValue(true);

        builder.Property(x => x.UseProjectNamingConventions)
            .HasDefaultValue(true);

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
        // EnvironmentDefinitions (OWNED)
        // ========================
        ConfigureEnvironments(builder);

        // ========================
        // ResourceNamingTemplates (Entity)
        // ========================
        builder.HasMany(x => x.ResourceNamingTemplates)
            .WithOne(x => x.InfraConfig)
            .HasForeignKey(x => x.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureEnvironments(
        EntityTypeBuilder<InfrastructureConfig> builder)
    {
        builder.OwnsMany(x => x.EnvironmentDefinitions, env =>
        {
            env.ToTable("Environments");

            env.HasKey(x => x.Id);

            env.Property(x => x.Id)
                .HasConversion(new IdValueConverter<EnvironmentDefinitionId>())
                .ValueGeneratedNever();

            env.Property(x => x.InfraConfigId)
                .HasConversion(new IdValueConverter<InfrastructureConfigId>());

            env.Property(x => x.Name)
                .HasConversion(new SingleValueConverter<Name, string>());

            env.Property(x => x.ShortName)
                .HasConversion(new SingleValueConverter<ShortName, string>());

            env.Property(x => x.Prefix)
                .HasConversion(new SingleValueConverter<Prefix, string>());

            env.Property(x => x.Suffix)
                .HasConversion(new SingleValueConverter<Suffix, string>());

            env.Property(x => x.Location)
                .HasConversion(new EnumValueConverter<Location, Location.LocationEnum>());

            env.Property(x => x.TenantId)
                .HasConversion(new SingleValueConverter<TenantId, Guid>());

            env.Property(x => x.SubscriptionId)
                .HasConversion(new SingleValueConverter<SubscriptionId, Guid>());

            env.Property(x => x.Order)
                .HasConversion(new SingleValueConverter<Order, int>());

            env.Property(x => x.RequiresApproval)
                .HasConversion(new SingleValueConverter<RequiresApproval, bool>());

            env.OwnsMany(x => x.Tags, tag =>
            {
                tag.ToTable("EnvironmentTags");
                tag.WithOwner().HasForeignKey("EnvironmentId");
                tag.HasKey("EnvironmentId", "Name");

                tag.Property(t => t.Name).HasMaxLength(100);
                tag.Property(t => t.Value).HasMaxLength(500);
            });

            // ========================
            // EnvironmentParameterValues (OWNED OF OWNED)
            // ========================
            env.OwnsMany(x => x.ParameterValues, pv =>
            {
                pv.ToTable("EnvironmentParameterValues");

                pv.HasKey(x => x.Id);

                pv.Property(x => x.Id)
                    .HasConversion(new IdValueConverter<EnvironmentParameterValueId>())
                    .ValueGeneratedNever();

                pv.Property(x => x.EnvironmentId)
                    .HasConversion(new IdValueConverter<EnvironmentDefinitionId>());

                pv.Property(x => x.ParameterId)
                    .HasConversion(new IdValueConverter<ParameterDefinitionId>());

                pv.Property(x => x.Value);
            });
        });
    }
}