using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using Shared.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public sealed class ParameterDefinitionConfiguration
    : IEntityTypeConfiguration<ParameterDefinition>
{
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
            .HasConversion(new SingleValueConverter<Name, string>());

        builder.Property(x => x.Type)
            .HasConversion(
                new EnumValueConverter<ParameterType, ParameterType.Enum>());

        builder.Property(x => x.IsSecret);

        builder.Property(x => x.DefaultValue);
    }
}