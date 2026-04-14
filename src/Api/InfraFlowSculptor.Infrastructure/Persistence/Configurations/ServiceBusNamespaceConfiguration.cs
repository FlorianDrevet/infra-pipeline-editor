using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ServiceBusNamespace"/> aggregate root (TPT).
/// </summary>
public class ServiceBusNamespaceConfiguration : IEntityTypeConfiguration<ServiceBusNamespace>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ServiceBusNamespace> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("ServiceBusNamespaces");

        builder.HasMany(sb => sb.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.ServiceBusNamespaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(sb => sb.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(sb => sb.Queues)
            .WithOne()
            .HasForeignKey(q => q.ServiceBusNamespaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(sb => sb.Queues)
            .HasField("_queues")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(sb => sb.TopicSubscriptions)
            .WithOne()
            .HasForeignKey(ts => ts.ServiceBusNamespaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(sb => sb.TopicSubscriptions)
            .HasField("_topicSubscriptions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
