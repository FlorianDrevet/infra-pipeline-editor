using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations.Converters;
using Shared.Infrastructure.Persistence.Configurations.Extensions;

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
            .HasConversion(new SingleValueConverter<Name, string>())
            .IsRequired(false);

        // Members
        builder.HasMany(x => x.Members)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configurations
        builder.HasMany(x => x.Configurations)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
