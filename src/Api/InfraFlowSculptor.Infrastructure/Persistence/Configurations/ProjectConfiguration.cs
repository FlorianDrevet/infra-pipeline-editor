using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Extensions;
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
#pragma warning disable CS8620 // Nullability mismatch — EF Core handles null conversion internally
            .HasConversion(new SingleValueConverter<NamingTemplate, string>())
#pragma warning restore CS8620
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

        // ========================
        // GitRepositoryConfiguration (Entity, 0..1)
        // ========================
        builder.HasOne(x => x.GitRepositoryConfiguration)
            .WithOne()
            .HasForeignKey<GitRepositoryConfiguration>(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ========================
        // PipelineVariableGroups (Entity)
        // ========================
        builder.HasMany(x => x.PipelineVariableGroups)
            .WithOne()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.PipelineVariableGroups)
            .HasField("_projectPipelineVariableGroups")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ========================
        // RepositoryMode
        // ========================
        builder.Property(x => x.RepositoryMode)
            .HasConversion(new EnumValueConverter<RepositoryMode, RepositoryModeEnum>())
            .HasDefaultValue(new RepositoryMode(RepositoryModeEnum.MultiRepo))
            .IsRequired();

        // ========================
        // Tags (OWNED)
        // ========================
        builder.OwnsMany(p => p.Tags, tag =>
        {
            tag.ToTable("ProjectTags");
            tag.WithOwner().HasForeignKey("ProjectId");
            tag.HasKey("ProjectId", "Name");
            tag.Property(t => t.Name).HasMaxLength(100);
            tag.Property(t => t.Value).HasMaxLength(500);
        });
        builder.Navigation(p => p.Tags).HasField("_tags").UsePropertyAccessMode(PropertyAccessMode.Field);
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

            env.Property(x => x.ShortName)
                .HasConversion(new SingleValueConverter<ShortName, string>());

            env.Property(x => x.Prefix)
                .HasConversion(new SingleValueConverter<Prefix, string>());

            env.Property(x => x.Suffix)
                .HasConversion(new SingleValueConverter<Suffix, string>());

            env.Property(x => x.Location)
                .HasConversion(new EnumValueConverter<Location, Location.LocationEnum>());

            env.Property(x => x.SubscriptionId)
                .HasConversion(new SingleValueConverter<SubscriptionId, Guid>());

            env.Property(x => x.Order)
                .HasConversion(new SingleValueConverter<Order, int>());

            env.Property(x => x.RequiresApproval)
                .HasConversion(new SingleValueConverter<RequiresApproval, bool>());

            env.Property(x => x.AzureResourceManagerConnection)
                .HasMaxLength(256);

            env.OwnsMany(x => x.Tags, tag =>
            {
                tag.ToTable("ProjectEnvironmentTags");
                tag.WithOwner().HasForeignKey("EnvironmentId");
                tag.HasKey("EnvironmentId", "Name");

                tag.Property(t => t.Name).HasMaxLength(100);
                tag.Property(t => t.Value).HasMaxLength(500);
            });

            env.Navigation(x => x.Tags).HasField("_tags").UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}