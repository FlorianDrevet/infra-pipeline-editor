using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class BlobLifecycleRuleConfiguration : IEntityTypeConfiguration<BlobLifecycleRule>
{
    public void Configure(EntityTypeBuilder<BlobLifecycleRule> builder)
    {
        builder.ToTable("BlobLifecycleRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(new IdValueConverter<BlobLifecycleRuleId>())
            .ValueGeneratedNever();

        builder.Property(r => r.StorageAccountId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(r => r.RuleName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.ContainerNames)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(r => r.TimeToLiveInDays)
            .IsRequired();
    }
}
