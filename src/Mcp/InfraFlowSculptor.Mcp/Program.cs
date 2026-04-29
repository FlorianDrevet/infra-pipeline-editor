using InfraFlowSculptor.Application;
using InfraFlowSculptor.Infrastructure;
using InfraFlowSculptor.Mcp.Common;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Imports;
using InfraFlowSculptor.Mcp.Imports.Resources;
using InfraFlowSculptor.Mcp.Prompts;
using InfraFlowSculptor.Mcp.Resources;
using InfraFlowSculptor.Mcp.Tools;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

const string McpRoute = "/mcp";

var builder = WebApplication.CreateBuilder(args);

var mcpUrl = builder.Configuration["Mcp:ListenUrl"] ?? "http://127.0.0.1:5258";
builder.WebHost.UseUrls(mcpUrl);

builder.Services.AddSingleton<IProjectDraftService, ProjectDraftService>();
builder.Services.AddSingleton<IImportPreviewService, ImportPreviewService>();
builder.Services.AddHostedService<InMemoryCleanupService>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
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
    .AddInfrastructure(builder.Configuration, builder.Environment, includeAuthentication: false)
    .AddPatAuthentication();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live")
}).AllowAnonymous();
app.MapMcp(McpRoute).RequireAuthorization();

await app.RunAsync();
