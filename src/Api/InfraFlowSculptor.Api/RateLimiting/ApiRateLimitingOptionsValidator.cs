using InfraFlowSculptor.Api.Options;
using InfraFlowSculptor.WebDefaults.RateLimiting;
using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Api.RateLimiting;

/// <summary>Validates <see cref="ApiRateLimitingOptions"/> including the HealthChecks policy.</summary>
internal sealed class ApiRateLimitingOptionsValidator : IValidateOptions<ApiRateLimitingOptions>
{
    private readonly RateLimitingOptionsValidator<ApiRateLimitingOptions> _baseValidator = new();

    public ValidateOptionsResult Validate(string? name, ApiRateLimitingOptions options)
    {
        var baseResult = _baseValidator.Validate(name, options);
        if (baseResult.Failed)
            return baseResult;

        List<string> failures = [];
        RateLimitingOptionsValidator<ApiRateLimitingOptions>.ValidatePolicy(
            options.HealthChecks, nameof(ApiRateLimitingOptions.HealthChecks), failures);

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}