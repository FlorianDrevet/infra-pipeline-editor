using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class CorsRuleConfiguration : IEntityTypeConfiguration<CorsRule>
{
    private const string TextArrayColumnType = "text[]";

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

        builder.Property<List<string>>("_allowedOrigins")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName(nameof(CorsRule.AllowedOrigins))
            .HasColumnType(TextArrayColumnType)
            .IsRequired();

        builder.Property<List<string>>("_allowedMethods")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName(nameof(CorsRule.AllowedMethods))
            .HasColumnType(TextArrayColumnType)
            .IsRequired();

        builder.Property<List<string>>("_allowedHeaders")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName(nameof(CorsRule.AllowedHeaders))
            .HasColumnType(TextArrayColumnType)
            .IsRequired();

        builder.Property<List<string>>("_exposedHeaders")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName(nameof(CorsRule.ExposedHeaders))
            .HasColumnType(TextArrayColumnType)
            .IsRequired();

        builder.Property(cr => cr.MaxAgeInSeconds)
            .IsRequired();
    }
}