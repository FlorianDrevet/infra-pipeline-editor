using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace InfraFlowSculptor.Infrastructure.Persistence.Conventions;

/// <summary>
/// EF Core convention that automatically applies value converters for domain
/// value objects (<see cref="Id{TId}"/> and <see cref="SingleValueObject{T}"/>)
/// so that individual <see cref="IEntityTypeConfiguration{TEntity}"/> classes
/// no longer need explicit <c>.HasConversion(…)</c> calls for these types.
/// </summary>
public sealed class ValueObjectConvention : IModelFinalizingConvention
{
    /// <inheritdoc />
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                var clrType = property.ClrType;
                if (clrType is null || property.GetValueConverter() is not null)
                    continue;

                if (TryGetIdConverter(clrType, out var idConverter))
                {
                    property.SetValueConverter(idConverter);
                }
                else if (TryGetSingleValueObjectConverter(clrType, out var svoConverter))
                {
                    property.SetValueConverter(svoConverter);
                }
            }
        }
    }

    private static bool TryGetIdConverter(Type clrType, out Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter)
    {
        converter = null;
        var type = clrType;
        while (type is not null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Id<>))
            {
                var converterType = typeof(IdValueConverter<>).MakeGenericType(clrType);
                converter = (Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter)Activator.CreateInstance(converterType)!;
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static bool TryGetSingleValueObjectConverter(Type clrType, out Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter? converter)
    {
        converter = null;
        var type = clrType;
        while (type is not null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SingleValueObject<>))
            {
                var innerType = type.GetGenericArguments()[0];
                var converterType = typeof(SingleValueConverter<,>).MakeGenericType(clrType, innerType);
                converter = (Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter)Activator.CreateInstance(converterType)!;
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }
}
