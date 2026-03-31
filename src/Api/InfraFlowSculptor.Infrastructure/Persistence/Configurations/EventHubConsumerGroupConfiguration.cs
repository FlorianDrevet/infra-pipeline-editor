using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="EventHubConsumerGroup"/> entity.
/// </summary>
public sealed class EventHubConsumerGroupConfiguration : IEntityTypeConfiguration<EventHubConsumerGroup>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EventHubConsumerGroup> builder)
    {
        builder.ToTable("EventHubConsumerGroups");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<EventHubConsumerGroupId>());

        builder.Property(x => x.EventHubNamespaceId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EventHubName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.ConsumerGroupName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.EventHubNamespaceId, x.EventHubName, x.ConsumerGroupName })
            .IsUnique();
    }
}
