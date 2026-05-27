using InfraFlowSculptor.Domain.Common.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

/// <summary>
/// Converts optional strongly typed identifiers to nullable <see cref="Guid"/> columns and back.
/// </summary>
/// <typeparam name="TId">The identifier type.</typeparam>
public sealed class NullableIdValueConverter<TId> : ValueConverter<TId?, Guid?>
    where TId : Id<TId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NullableIdValueConverter{TId}"/> class.
    /// </summary>
    public NullableIdValueConverter()
        : base(
            id => id == null ? null : id.Value,
            value => value.HasValue ? Id<TId>.Create(value.Value) : null)
    {
    }
}
