using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public sealed class ParameterDefinitionConfiguration
    : IEntityTypeConfiguration<ParameterDefinition>
{
    private const int ParameterNameMaxLength = 100;
    private const int ParameterTypeMaxLength = 20;
    private const int DefaultValueMaxLength = 500;

    public void Configure(EntityTypeBuilder<ParameterDefinition> builder)
    {
        builder.ToTable("ParameterDefinitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ParameterDefinitionId>())
            .ValueGeneratedNever();

        builder.Property(x => x.InfraConfigId)
            .HasConversion(new IdValueConverter<InfrastructureConfigId>());
        
        builder.Property(x => x.IsSecret)
            .HasConversion(new SingleValueConverter<IsSecret, bool>());

        builder.Property(x => x.Name)
            .HasConversion(new SingleValueConverter<Name, string>())
            .HasMaxLength(ParameterNameMaxLength);

        builder.Property(x => x.Type)
            .HasConversion(
                new EnumValueConverter<ParameterType, ParameterType.Enum>())
            .HasMaxLength(ParameterTypeMaxLength);

        builder.Property(x => x.IsSecret);

        builder.Property(x => x.DefaultValue)
            .HasMaxLength(DefaultValueMaxLength);
    }
}