using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

public sealed class ParameterUsage : ValueObject
{
    public string Value { get; }

    private ParameterUsage(string value)
    {
        Value = value;
    }

    public static ParameterUsage Secret => new("secret");
    public static ParameterUsage AppSetting => new("appSetting");
    public static ParameterUsage ConnectionString => new("connectionString");
    
    public static IEnumerable<ParameterUsage> All =>
        [Secret, AppSetting, ConnectionString];

    public static ParameterUsage From(string value)
    {
        var usage = All.FirstOrDefault(x => x.Value == value);
        if (usage is null)
            throw new ArgumentException($"Invalid ParameterUsage '{value}'");

        return usage;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}