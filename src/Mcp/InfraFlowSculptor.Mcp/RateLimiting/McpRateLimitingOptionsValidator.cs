using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Mcp.RateLimiting;

internal sealed class McpRateLimitingOptionsValidator : IValidateOptions<McpRateLimitingOptions>
{
    public ValidateOptionsResult Validate(string? name, McpRateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [];

        ValidatePolicy(options.Global, nameof(McpRateLimitingOptions.Global), failures);
        ValidatePolicy(options.Expensive, nameof(McpRateLimitingOptions.Expensive), failures);

        if (options.Expensive.PermitLimit > options.Global.PermitLimit)
        {
            failures.Add("Expensive.PermitLimit must be less than or equal to Global.PermitLimit.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidatePolicy(
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