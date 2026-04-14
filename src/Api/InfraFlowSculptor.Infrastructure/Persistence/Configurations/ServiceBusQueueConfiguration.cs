using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ServiceBusQueue"/> entity.
/// </summary>
public sealed class ServiceBusQueueConfiguration : IEntityTypeConfiguration<ServiceBusQueue>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ServiceBusQueue> builder)
    {
        builder.ToTable("ServiceBusQueues");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ServiceBusQueueId>());

        builder.Property(x => x.ServiceBusNamespaceId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(260);

        builder.HasIndex(x => new { x.ServiceBusNamespaceId, x.Name })
            .IsUnique();
    }
}
