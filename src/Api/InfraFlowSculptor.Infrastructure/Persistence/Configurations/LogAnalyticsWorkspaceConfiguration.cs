using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="LogAnalyticsWorkspace"/> aggregate root (TPT).
/// </summary>
public sealed class LogAnalyticsWorkspaceConfiguration : IEntityTypeConfiguration<LogAnalyticsWorkspace>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LogAnalyticsWorkspace> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("LogAnalyticsWorkspaces");

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.LogAnalyticsWorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
