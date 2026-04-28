using InfraFlowSculptor.Application;
using InfraFlowSculptor.Infrastructure;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Imports;
using InfraFlowSculptor.Mcp.Imports.Resources;
using InfraFlowSculptor.Mcp.Prompts;
using InfraFlowSculptor.Mcp.Resources;
using InfraFlowSculptor.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<IProjectDraftService, ProjectDraftService>();
builder.Services.AddSingleton<IImportPreviewService, ImportPreviewService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<DiscoveryTools>()
    .WithTools<ProjectDraftTools>()
    .WithTools<ProjectCreationTools>()
    .WithTools<BicepGenerationTools>()
    .WithTools<IacImportTools>()
    .WithResources<ProjectResources>()
    .WithResources<ImportPreviewResources>()
    .WithPrompts<ProjectCreationPrompts>();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();
await app.RunAsync();
