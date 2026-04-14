using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="EventHub"/> entity.
/// </summary>
public sealed class EventHubConfiguration : IEntityTypeConfiguration<EventHub>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EventHub> builder)
    {
        builder.ToTable("EventHubs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<EventHubId>());

        builder.Property(x => x.EventHubNamespaceId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(260);

        builder.HasIndex(x => new { x.EventHubNamespaceId, x.Name })
            .IsUnique();
    }
}
