using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="EventHubNamespace"/> aggregate root (TPT).</summary>
public class EventHubNamespaceConfiguration : IEntityTypeConfiguration<EventHubNamespace>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EventHubNamespace> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("EventHubNamespaces");

        builder.HasMany(eh => eh.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.EventHubNamespaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(eh => eh.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(eh => eh.EventHubs)
            .WithOne()
            .HasForeignKey(e => e.EventHubNamespaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(eh => eh.EventHubs)
            .HasField("_eventHubs")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(eh => eh.ConsumerGroups)
            .WithOne()
            .HasForeignKey(cg => cg.EventHubNamespaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(eh => eh.ConsumerGroups)
            .HasField("_consumerGroups")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
