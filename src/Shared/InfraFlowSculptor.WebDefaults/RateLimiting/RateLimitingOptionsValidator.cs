using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.WebDefaults.RateLimiting;

/// <summary>
/// Validates any <see cref="IRateLimitingOptions"/> implementation at startup.
/// </summary>
/// <typeparam name="TOptions">The concrete options type to validate.</typeparam>
public class RateLimitingOptionsValidator<TOptions> : IValidateOptions<TOptions>
    where TOptions : class, IRateLimitingOptions
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [];

        ValidatePolicy(options.Global, nameof(IRateLimitingOptions.Global), failures);
        ValidatePolicy(options.Expensive, nameof(IRateLimitingOptions.Expensive), failures);

        if (options.Expensive.PermitLimit > options.Global.PermitLimit)
        {
            failures.Add("Expensive.PermitLimit must be less than or equal to Global.PermitLimit.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    /// <summary>Validates a single fixed-window policy.</summary>
    public static void ValidatePolicy(
        FixedWindowRateLimitingPolicyOptions policy,
        string policyName,
        ICollection<string> failures)
    {
        if (policy.PermitLimit <= 0)
        {
            failures.Add($"{policyName}.PermitLimit must be greater than 0.");
        }

        if (policy.WindowSeconds <= 0)
        {
            failures.Add($"{policyName}.WindowSeconds must be greater than 0.");
        }

        if (policy.QueueLimit < 0)
        {
            failures.Add($"{policyName}.QueueLimit must be greater than or equal to 0.");
        }
    }
}
