using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ServiceBusTopicSubscription"/> entity.
/// </summary>
public sealed class ServiceBusTopicSubscriptionConfiguration : IEntityTypeConfiguration<ServiceBusTopicSubscription>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ServiceBusTopicSubscription> builder)
    {
        builder.ToTable("ServiceBusTopicSubscriptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ServiceBusTopicSubscriptionId>());

        builder.Property(x => x.ServiceBusNamespaceId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.TopicName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.SubscriptionName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.ServiceBusNamespaceId, x.TopicName, x.SubscriptionName })
            .IsUnique();
    }
}
