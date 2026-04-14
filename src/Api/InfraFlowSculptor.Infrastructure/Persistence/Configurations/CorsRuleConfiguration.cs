using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class CorsRuleConfiguration : IEntityTypeConfiguration<CorsRule>
{
    public void Configure(EntityTypeBuilder<CorsRule> builder)
    {
        builder.ToTable("StorageAccountCorsRules");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Id)
            .HasConversion(new IdValueConverter<CorsRuleId>())
            .ValueGeneratedNever();

        builder.Property(cr => cr.StorageAccountId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(cr => cr.ServiceType)
            .HasConversion(new EnumValueConverter<CorsServiceType, CorsServiceType.Service>())
            .IsRequired();

        builder.Property(cr => cr.AllowedOrigins)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(cr => cr.AllowedMethods)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(cr => cr.AllowedHeaders)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(cr => cr.ExposedHeaders)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(cr => cr.MaxAgeInSeconds)
            .IsRequired();
    }
}