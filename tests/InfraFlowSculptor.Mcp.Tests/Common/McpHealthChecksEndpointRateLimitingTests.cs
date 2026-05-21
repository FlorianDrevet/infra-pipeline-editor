using FluentAssertions;
using InfraFlowSculptor.Mcp.Common;
using InfraFlowSculptor.Mcp.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InfraFlowSculptor.Mcp.Tests.Common;

public sealed class McpHealthChecksEndpointRateLimitingTests
{
    [Fact]
    public void Given_McpHealthChecksAreMapped_When_InspectingEndpointMetadata_Then_BothEndpointsUseTheHealthChecksPolicy()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
        });

        builder.Services.AddHealthChecks();
        builder.Services.AddMcpRateLimiting();

        var application = builder.Build();
        application.UseRouting();

        // Act
        application.MapMcpHealthChecks();

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