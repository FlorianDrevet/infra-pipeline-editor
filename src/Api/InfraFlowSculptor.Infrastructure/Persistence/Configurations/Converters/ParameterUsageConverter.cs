using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

public sealed class ParameterUsageConverter
    : ValueConverter<ParameterUsage, string>
{
    public ParameterUsageConverter()
        : base(
            usage => usage.Value,
            value => ParameterUsage.From(value))
    {
    }
}
