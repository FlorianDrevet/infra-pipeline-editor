using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Net;
using FluentAssertions;
using InfraFlowSculptor.Api.Controllers;
using InfraFlowSculptor.Api.RateLimiting;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using NSubstitute;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.RateLimiting;

public sealed class RateLimitingTests
{
    private const string RateLimitingSectionName = "RateLimiting";
    private const string GlobalPermitLimitKey = $"{RateLimitingSectionName}:Global:PermitLimit";
    private const string GlobalWindowSecondsKey = $"{RateLimitingSectionName}:Global:WindowSeconds";
    private const string ExpensivePermitLimitKey = $"{RateLimitingSectionName}:Expensive:PermitLimit";
    private const string ExpensiveWindowSecondsKey = $"{RateLimitingSectionName}:Expensive:WindowSeconds";
    private const string FirstUserId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    private const string SecondUserId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    private static readonly string[] ExpectedExpensiveEndpointNames =
    [
        "GenerateBicep",
        "DownloadBicep",
        "PushBicepToGit",
        "GeneratePipeline",
        "DownloadPipeline",
        "PushPipelineToGit",
        "GenerateProjectBicep",
        "DownloadProjectBicep",
        "PushProjectBicepToGit",
        "GenerateProjectPipeline",
        "DownloadProjectPipeline",
        "PushProjectPipelineToGit",
        "GenerateProjectBootstrapPipeline",
        "DownloadProjectBootstrapPipeline",
        "PushProjectBootstrapPipelineToGit",
        "PushProjectGeneratedArtifactsToGit",
        "PushProjectArtifactsToMultiRepo",
    ];

    [Fact]
    public async Task Given_RequestCountWithinGlobalLimit_When_SendingRequests_Then_AllResponsesAreSuccessfulAsync()
    {
        // Arrange
        await using var host = await RateLimitingTestHost.CreateAsync(CreateSettings(globalPermitLimit: 2, expensivePermitLimit: 1));

        // Act
        var firstResponse = await host.Client.GetAsync("/global");
        var secondResponse = await host.Client.GetAsync("/global");

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_RequestCountExceedsGlobalLimit_When_SendingRequests_Then_ReturnsTooManyRequestsAsync()
    {
        // Arrange
        await using var host = await RateLimitingTestHost.CreateAsync(CreateSettings(globalPermitLimit: 2, expensivePermitLimit: 1));

        // Act
        _ = await host.Client.GetAsync("/global");
        _ = await host.Client.GetAsync("/global");
        var rejectedResponse = await host.Client.GetAsync("/global");

        // Assert
        rejectedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Given_GlobalLimitRejectsRequest_When_SendingRequests_Then_ExposesRetryAfterHeaderAsync()
    {
        // Arrange
        await using var host = await RateLimitingTestHost.CreateAsync(CreateSettings(globalPermitLimit: 1, expensivePermitLimit: 1));

        // Act
        _ = await host.Client.GetAsync("/global");
        var rejectedResponse = await host.Client.GetAsync("/global");

        // Assert
        rejectedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rejectedResponse.Headers.RetryAfter.Should().NotBeNull();
        rejectedResponse.Headers.RetryAfter!.Delta.Should().NotBeNull();
        rejectedResponse.Headers.RetryAfter!.Delta!.Value.TotalSeconds.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Given_ExpensivePolicyHasLowerPermitLimit_When_SendingRequests_Then_ItRejectsSoonerThanGlobalPolicyAsync()
    {
        // Arrange
        await using var host = await RateLimitingTestHost.CreateAsync(CreateSettings(globalPermitLimit: 3, expensivePermitLimit: 1));

        // Act
        var firstResponse = await host.Client.GetAsync("/expensive");
        var secondResponse = await host.Client.GetAsync("/expensive");

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Given_DistinctAuthenticatedUsersSharingSameIp_When_SendingRequests_Then_TheyReceiveIndependentGlobalBucketsAsync()
    {
        // Arrange
        await using var host = await RateLimitingTestHost.CreateAsync(CreateSettings(globalPermitLimit: 1, expensivePermitLimit: 1));

        // Act
        var firstUserInitialResponse = await host.SendAuthenticatedRequestAsync("/global", FirstUserId);
        var firstUserRejectedResponse = await host.SendAuthenticatedRequestAsync("/global", FirstUserId);
        var secondUserResponse = await host.SendAuthenticatedRequestAsync("/global", SecondUserId);

        // Assert
        firstUserInitialResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstUserRejectedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        secondUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_AuthenticatedRequestsWithoutStableClaims_When_SendingRequests_Then_TheyFallBackToIpBucketAsync()
    {
        // Arrange
        await using var host = await RateLimitingTestHost.CreateAsync(CreateSettings(globalPermitLimit: 1, expensivePermitLimit: 1));

        // Act
        var firstResponse = await host.SendDisplayNameOnlyAuthenticatedRequestAsync("/global", "Alpha");
        var secondResponse = await host.SendDisplayNameOnlyAuthenticatedRequestAsync("/global", "Beta");

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public void Given_HeavyGenerationEndpoints_When_BuildingEndpointMap_Then_AllRequireExpensivePolicy()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
        });

        var mediator = Substitute.For<IMediator>();

        builder.Services.AddSingleton(mediator);
        builder.Services.AddSingleton<ISender>(mediator);
        builder.Services.AddSingleton(Substitute.For<IMapper>());

        var application = builder.Build();
        application.UseRouting();
        application.UseBicepGenerationController();
        application.UsePipelineGenerationController();
        application.UseProjectController();
        application.UseProjectGenerationController();

        var endpoints = application.Services
            .GetRequiredService<IEnumerable<EndpointDataSource>>()
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();

        // Assert
        var expensiveEndpointNames = endpoints
            .Where(endpoint => endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName == RateLimitingPolicyNames.Expensive)
            .Select(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName)
            .Where(endpointName => !string.IsNullOrWhiteSpace(endpointName))
            .Cast<string>()
            .ToArray();

        expensiveEndpointNames.Should().BeEquivalentTo(ExpectedExpensiveEndpointNames);
    }

    [Theory]
    [MemberData(nameof(GetInvalidConfigurations))]
    public async Task Given_InvalidRateLimitingConfiguration_When_StartingApplication_Then_ThrowsOptionsValidationExceptionAsync(
        IReadOnlyDictionary<string, string?> settings)
    {
        // Arrange
        Func<Task> act = async () => _ = await RateLimitingTestHost.CreateAsync(settings);

        // Act
        var assertion = await act.Should().ThrowAsync<OptionsValidationException>();

        // Assert
        assertion.Which.OptionsName.Should().Be(Microsoft.Extensions.Options.Options.DefaultName);
    }

    public static TheoryData<IReadOnlyDictionary<string, string?>> GetInvalidConfigurations()
    {
        return
        [
            CreateSettings(globalPermitLimit: 0, expensivePermitLimit: 1),
            CreateSettings(globalPermitLimit: 2, expensivePermitLimit: 3),
        ];
    }

    private static Dictionary<string, string?> CreateSettings(int globalPermitLimit, int expensivePermitLimit, int windowSeconds = 5)
    {
        return new Dictionary<string, string?>
        {
            [GlobalPermitLimitKey] = globalPermitLimit.ToString(),
            [GlobalWindowSecondsKey] = windowSeconds.ToString(),
            [ExpensivePermitLimitKey] = expensivePermitLimit.ToString(),
            [ExpensiveWindowSecondsKey] = windowSeconds.ToString(),
        };
    }

    private sealed class RateLimitingTestHost : IAsyncDisposable
    {
        private readonly WebApplication _application;

        private RateLimitingTestHost(WebApplication application)
        {
            _application = application;
            Client = application.GetTestClient();
        }

        public HttpClient Client { get; }

        public async Task<HttpResponseMessage> SendAuthenticatedRequestAsync(string path, string userId)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Add(TestAuthenticationHandler.UserIdHeaderName, userId);
            return await Client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> SendDisplayNameOnlyAuthenticatedRequestAsync(string path, string displayName)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Add(TestAuthenticationHandler.DisplayNameHeaderName, displayName);
            return await Client.SendAsync(request);
        }

        public static async Task<RateLimitingTestHost> CreateAsync(IReadOnlyDictionary<string, string?> settings)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = Environments.Development,
            });

            builder.WebHost.UseTestServer();
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(settings);
            builder.Services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, _ => { });
            builder.Services.AddRateLimiting();

            var application = builder.Build();

            application.UseRouting();
            application.Use((context, next) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
                return next();
            });
            application.UseAuthentication();
            application.UseRateLimiter();

            application.MapGet("/global", () => Results.Ok())
                .WithName("GlobalTestEndpoint");

            application.MapGet("/expensive", () => Results.Ok())
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName("ExpensiveTestEndpoint");

            await application.StartAsync();

            return new RateLimitingTestHost(application);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _application.StopAsync();
            await _application.DisposeAsync();
        }
    }

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";
        public const string UserIdHeaderName = "X-Test-User-Id";
        public const string DisplayNameHeaderName = "X-Test-Display-Name";

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.TryGetValue(UserIdHeaderName, out var userIds) && !string.IsNullOrWhiteSpace(userIds[0]))
            {
                var userId = userIds[0]!;
                var stableClaims = new[]
                {
                    new Claim(ClaimConstants.ObjectId, userId),
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, $"User {userId}"),
                };

                return Task.FromResult(CreateSuccessResult(stableClaims));
            }

            if (!Request.Headers.TryGetValue(DisplayNameHeaderName, out var displayNames) || string.IsNullOrWhiteSpace(displayNames[0]))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var displayName = displayNames[0]!;
            var displayNameOnlyClaims = new[]
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimConstants.Name, displayName),
            };

            return Task.FromResult(CreateSuccessResult(displayNameOnlyClaims));
        }

        private AuthenticateResult CreateSuccessResult(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
