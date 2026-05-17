using FluentAssertions;
using InfraFlowSculptor.Api.Configuration;
using InfraFlowSculptor.Api.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.RateLimiting;

public sealed class HealthChecksEndpointRateLimitingTests
{
    [Fact]
    public void Given_ApiHealthChecksAreMapped_When_InspectingEndpointMetadata_Then_BothEndpointsUseTheHealthChecksPolicy()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
        });

        builder.Services.AddHealthChecks();

        var application = builder.Build();
        application.UseRouting();

        // Act
        application.MapApiHealthChecks();

        var endpointRouteBuilder = (IEndpointRouteBuilder)application;

        var endpoints = endpointRouteBuilder.DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText is "/health" or "/alive")
            .ToArray();

        var appliedPolicyNames = endpoints
            .Select(endpoint => endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName)
            .ToArray();

        // Assert
        endpoints.Should().HaveCount(2);
        appliedPolicyNames.Should().OnlyContain(policyName => policyName == RateLimitingPolicyNames.HealthChecks);
    }
}
