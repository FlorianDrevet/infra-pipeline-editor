using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using Shared.Infrastructure.Persistence.Configurations.Converters;
using Shared.Infrastructure.Persistence.Configurations.Extensions;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Project"/> aggregate.</summary>
public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    private const string TableName = "Projects";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);
        builder.ConfigureAggregateRootId<Project, ProjectId>();

        builder.Property(x => x.Name)
            .HasConversion(new SingleValueConverter<Name, string>())
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        // ========================
        // DefaultNamingTemplate
        // ========================
        builder.Property(x => x.DefaultNamingTemplate)
            .HasConversion(new SingleValueConverter<NamingTemplate, string>())
            .IsRequired(false);

        // ========================
        // Members (Entity)
        // ========================
        builder.HasMany(x => x.Members)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ========================
        // EnvironmentDefinitions (OWNED)
        // ========================
        ConfigureEnvironments(builder);

        // ========================
        // ResourceNamingTemplates (Entity)
        // ========================
        builder.HasMany(x => x.ResourceNamingTemplates)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureEnvironments(EntityTypeBuilder<Project> builder)
    {
        builder.OwnsMany(x => x.EnvironmentDefinitions, env =>
        {
            env.ToTable("ProjectEnvironments");

            env.HasKey(x => x.Id);

            env.Property(x => x.Id)
                .HasConversion(new IdValueConverter<ProjectEnvironmentDefinitionId>())
                .ValueGeneratedNever();

            env.Property(x => x.ProjectId)
                .HasConversion(new IdValueConverter<ProjectId>());

            env.Property(x => x.Name)
                .HasConversion(new SingleValueConverter<Name, string>());

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
                tag.ToTable("ProjectEnvironmentTags");
                tag.WithOwner().HasForeignKey("EnvironmentId");
                tag.HasKey("EnvironmentId", "Name");

                tag.Property(t => t.Name).HasMaxLength(100);
                tag.Property(t => t.Value).HasMaxLength(500);
            });
        });
    }
}
