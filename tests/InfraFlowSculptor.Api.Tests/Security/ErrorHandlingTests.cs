using System.Diagnostics;
using System.IO;
using FluentAssertions;
using InfraFlowSculptor.Api.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Security;

public sealed class ErrorHandlingTests
{
    [Fact]
    public async Task Given_UnhandledException_When_UsingErrorHandling_Then_ReturnsGenericProblemDetailsAsync()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddMetrics()
            .AddSingleton<DiagnosticListener>(_ => new DiagnosticListener("ErrorHandlingTests"))
            .AddSingleton<DiagnosticSource>(serviceProvider => serviceProvider.GetRequiredService<DiagnosticListener>())
            .BuildServiceProvider();
        var sut = CreateSut(services);
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
        };
        httpContext.Request.Path = "/api/projects";
        httpContext.Response.Body = new MemoryStream();

        // Act
        await sut(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        httpContext.Response.Body.Position = 0;
        var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("An error occurred.");
        responseBody.Should().NotContain("secret exception details");
    }

    [Fact]
    public async Task Given_UnhandledException_When_UsingErrorHandling_Then_LogsUnhandledExceptionAsync()
    {
        // Arrange
        var loggerProvider = new TestLoggerProvider();
        var services = new ServiceCollection()
            .AddLogging(loggingBuilder => loggingBuilder.AddProvider(loggerProvider))
            .AddMetrics()
            .AddSingleton<DiagnosticListener>(_ => new DiagnosticListener("ErrorHandlingTests"))
            .AddSingleton<DiagnosticSource>(serviceProvider => serviceProvider.GetRequiredService<DiagnosticListener>())
            .BuildServiceProvider();
        var sut = CreateSut(services);
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
        };
        httpContext.Request.Path = "/api/projects";
        httpContext.Response.Body = new MemoryStream();

        // Act
        await sut(httpContext);

        // Assert
        loggerProvider.Entries.Should().Contain(entry =>
            entry.LogLevel == LogLevel.Error
            && entry.Exception != null
            && entry.Exception.GetType() == typeof(InvalidOperationException)
            && entry.Exception.Message == "secret exception details");
    }

    [Fact]
    public async Task Given_UnhandledExceptionWithCurrentActivity_When_UsingErrorHandling_Then_EmitsTraceIdentifierAsync()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddMetrics()
            .AddSingleton<DiagnosticListener>(_ => new DiagnosticListener("ErrorHandlingTests"))
            .AddSingleton<DiagnosticSource>(serviceProvider => serviceProvider.GetRequiredService<DiagnosticListener>())
            .BuildServiceProvider();
        var sut = CreateSut(services);
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
        };
        httpContext.Request.Path = "/api/projects";
        httpContext.Response.Body = new MemoryStream();

        using var activity = new Activity("unhandled-error-test");
        activity.Start();
        var expectedTraceId = activity.Id;

        // Act
        await sut(httpContext);

        // Assert
        httpContext.Response.Body.Position = 0;
        var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("traceId");
        responseBody.Should().Contain(expectedTraceId);
    }

    private static RequestDelegate CreateSut(IServiceProvider services)
    {
        var applicationBuilder = new ApplicationBuilder(services);
        applicationBuilder.UseErrorHandling();
        applicationBuilder.Run(_ => throw new InvalidOperationException("secret exception details"));
        return applicationBuilder.Build();
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