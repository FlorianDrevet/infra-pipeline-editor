using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using InfraFlowSculptor.Mcp.Common;
using InfraFlowSculptor.Mcp.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace InfraFlowSculptor.Mcp.Tests.Common;

public sealed class McpHttpPipelineExtensionsTests
{
    private const string DevelopmentEnvironmentName = "Development";
    private const string RateLimitingSectionName = "RateLimiting";
    private const string GlobalPermitLimitKey = $"{RateLimitingSectionName}:Global:PermitLimit";
    private const string GlobalWindowSecondsKey = $"{RateLimitingSectionName}:Global:WindowSeconds";
    private const string ExpensivePermitLimitKey = $"{RateLimitingSectionName}:Expensive:PermitLimit";
    private const string ExpensiveWindowSecondsKey = $"{RateLimitingSectionName}:Expensive:WindowSeconds";

    [Fact]
    public async Task Given_HardenedMcpPipeline_When_RequestSucceeds_Then_AppliesSecurityHeadersAsync()
    {
        // Arrange
        await using var host = await McpPipelineTestHost.CreateAsync(CreateSettings(globalPermitLimit: 2, expensivePermitLimit: 1));

        // Act
        var response = await host.SendAuthenticatedRequestAsync("/secured", "user-alpha");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Frame-Options").Should().ContainSingle().Which.Should().Be("DENY");
        response.Headers.GetValues("X-Content-Type-Options").Should().ContainSingle().Which.Should().Be("nosniff");
        response.Headers.GetValues("Content-Security-Policy").Should().ContainSingle().Which.Should().Be("default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'");
    }

    [Fact]
    public async Task Given_HardenedMcpPipeline_When_GlobalLimitIsExceeded_Then_ReturnsTooManyRequestsAsync()
    {
        // Arrange
        await using var host = await McpPipelineTestHost.CreateAsync(CreateSettings(globalPermitLimit: 1, expensivePermitLimit: 1));

        // Act
        var firstResponse = await host.SendAuthenticatedRequestAsync("/secured", "user-alpha");
        var secondResponse = await host.SendAuthenticatedRequestAsync("/secured", "user-alpha");

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        secondResponse.Headers.RetryAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_NonDevelopmentPlainHttpListenUrl_When_UseMcpHttpPipeline_Then_LogsWarningAsync()
    {
        // Arrange
        var loggerProvider = new TestLoggerProvider();
        var settings = CreateSettings(globalPermitLimit: 2, expensivePermitLimit: 1);
        settings[$"{McpOptions.SectionName}:{nameof(McpOptions.ListenUrl)}"] = "http://127.0.0.1:5258";

        // Act
        await using var host = await McpPipelineTestHost.CreateAsync(
            settings,
            environmentName: Environments.Production,
            loggerProvider: loggerProvider);

        // Assert
        loggerProvider.Entries.Should().Contain(entry =>
            entry.LogLevel == LogLevel.Warning
            && entry.Message.Contains("plain HTTP", StringComparison.OrdinalIgnoreCase)
            && entry.Message.Contains("outside Development", StringComparison.OrdinalIgnoreCase));
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

    private sealed class McpPipelineTestHost : IAsyncDisposable
    {
        private readonly WebApplication _application;

        private McpPipelineTestHost(WebApplication application)
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

        public static async Task<McpPipelineTestHost> CreateAsync(
            IReadOnlyDictionary<string, string?> settings,
            string environmentName = DevelopmentEnvironmentName,
            ILoggerProvider? loggerProvider = null)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = environmentName,
            });

            builder.WebHost.UseTestServer();
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(settings);
            builder.Services.Configure<McpOptions>(builder.Configuration.GetSection(McpOptions.SectionName));
            builder.Services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, _ => { });
            builder.Services.AddAuthorization();
            builder.Services.AddMcpRateLimiting();

            if (loggerProvider is not null)
            {
                builder.Logging.ClearProviders();
                builder.Logging.AddProvider(loggerProvider);
            }

            var application = builder.Build();

            application.Use(async (context, next) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
                await next();
            });
            application.UseMcpHttpPipeline();

            application.MapGet("/secured", () => Results.Ok())
                .RequireAuthorization()
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive);

            await application.StartAsync();

            return new McpPipelineTestHost(application);
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

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserIdHeaderName, out var userIds) || string.IsNullOrWhiteSpace(userIds[0]))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var userId = userIds[0]!;
            var claims = new[]
            {
                new Claim(ClaimConstants.ObjectId, userId),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, $"User {userId}"),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    private sealed record LogEntry(LogLevel LogLevel, string Message, Exception? Exception);

    private sealed class TestLoggerProvider : ILoggerProvider
    {
        public IList<LogEntry> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(Entries);
        }

        public void Dispose()
        {
        }

        private sealed class TestLogger(IList<LogEntry> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
            }
        }
    }
}