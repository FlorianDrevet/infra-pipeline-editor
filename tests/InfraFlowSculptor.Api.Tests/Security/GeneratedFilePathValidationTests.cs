using System.Net;
using FluentAssertions;
using InfraFlowSculptor.Api.Controllers;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBicepFileContent;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetPipelineFileContent;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Security;

public sealed class GeneratedFilePathValidationTests
{
    [Theory]
    [InlineData("GetBicepFileContent", "../secrets.txt")]
    [InlineData("GetBicepFileContent", "..\\secrets.txt")]
    [InlineData("GetPipelineFileContent", "../secrets.txt")]
    [InlineData("GetPipelineFileContent", "C:/windows/system32")]
    public async Task Given_InvalidGeneratedFilePath_When_RequestingConfigArtifactContent_Then_ReturnsBadRequestAsync(
        string endpointName,
        string filePath)
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetBicepFileContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetBicepFileContentResult("content"));
        mediator.Send(Arg.Any<GetPipelineFileContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetPipelineFileContentResult("content"));

        await using var host = await ControllerTestHost.CreateAsync(mediator);

        // Act
        var response = await host.InvokeEndpointAsync(
            endpointName,
            new RouteValueDictionary
            {
                ["configId"] = Guid.NewGuid().ToString(),
                ["filePath"] = filePath,
            });

        // Assert
        response.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        mediator.ReceivedCalls().Should().BeEmpty();
    }

    [Theory]
    [InlineData("GetProjectBicepFileContent")]
    [InlineData("GetProjectPipelineFileContent")]
    [InlineData("GetProjectBootstrapPipelineFileContent")]
    public async Task Given_InvalidGeneratedFilePath_When_RequestingProjectArtifactContent_Then_ReturnsBadRequestAsync(string endpointName)
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        await using var host = await ControllerTestHost.CreateAsync(mediator);

        // Act
        var response = await host.InvokeEndpointAsync(
            endpointName,
            new RouteValueDictionary
            {
                ["projectId"] = Guid.NewGuid().ToString(),
                ["filePath"] = "../secrets.txt",
            });

        // Assert
        response.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        mediator.ReceivedCalls().Should().BeEmpty();
    }

    private sealed class ControllerTestHost : IAsyncDisposable
    {
        private readonly WebApplication _application;
        private readonly IReadOnlyDictionary<string, RouteEndpoint> _endpoints;

        private ControllerTestHost(WebApplication application)
        {
            _application = application;
            _endpoints = application.Services
                .GetRequiredService<IEnumerable<EndpointDataSource>>()
                .SelectMany(dataSource => dataSource.Endpoints)
                .OfType<RouteEndpoint>()
                .Where(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName is not null)
                .ToDictionary(
                    endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName!,
                    endpoint => endpoint);
        }

        public static async Task<ControllerTestHost> CreateAsync(IMediator mediator)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = Environments.Development,
            });

            builder.WebHost.UseTestServer();
            builder.Services.AddSingleton(mediator);
            builder.Services.AddSingleton<ISender>(mediator);
            builder.Services.AddSingleton(Substitute.For<IMapper>());

            var application = builder.Build();
            application.UseRouting();
            application.UseBicepGenerationController();
            application.UsePipelineGenerationController();
            application.UseProjectController();
            application.UseProjectGenerationController();

            await application.StartAsync();

            return new ControllerTestHost(application);
        }

        public async Task<DefaultHttpContext> InvokeEndpointAsync(string endpointName, RouteValueDictionary routeValues)
        {
            var endpoint = _endpoints[endpointName];
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _application.Services,
            };

            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.RouteValues = routeValues;
            httpContext.Response.Body = new MemoryStream();
            httpContext.SetEndpoint(endpoint);

            await endpoint.RequestDelegate!(httpContext);

            return httpContext;
        }

        public async ValueTask DisposeAsync()
        {
            await _application.StopAsync();
            await _application.DisposeAsync();
        }
    }
}
