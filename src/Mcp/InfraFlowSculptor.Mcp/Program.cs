using InfraFlowSculptor.Application;
using InfraFlowSculptor.Infrastructure;
using InfraFlowSculptor.Mcp.Common;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Imports;
using InfraFlowSculptor.Mcp.Imports.Resources;
using InfraFlowSculptor.Mcp.Prompts;
using InfraFlowSculptor.Mcp.RateLimiting;
using InfraFlowSculptor.Mcp.Resources;
using InfraFlowSculptor.Mcp.Tools;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var mcpOptionsSection = builder.Configuration.GetSection(McpOptions.SectionName);
builder.Services.Configure<McpOptions>(mcpOptionsSection);
builder.Services.Configure<ProjectDraftStorageOptions>(builder.Configuration.GetSection(ProjectDraftStorageOptions.SectionName));
builder.Services.Configure<ImportPreviewStorageOptions>(builder.Configuration.GetSection(ImportPreviewStorageOptions.SectionName));

var mcpOptions = mcpOptionsSection.Get<McpOptions>() ?? new McpOptions();
builder.WebHost.UseUrls(mcpOptions.ListenUrl);

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
    .WithTools<ProjectManagementTools>()
    .WithTools<InfrastructureTools>()
    .WithTools<ResourceCreationTools>()
    .WithTools<ResourceConfigurationTools>()
    .WithTools<RoleAssignmentTools>()
    .WithTools<AppSettingsTools>()
    .WithTools<SubResourceTools>()
    .WithTools<NamingTools>()
    .WithTools<BicepGenerationTools>()
    .WithTools<IacImportTools>()
    .WithResources<ProjectResources>()
    .WithResources<ImportPreviewResources>()
    .WithPrompts<ProjectCreationPrompts>();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment, includeAuthentication: false)
    .AddPatAuthentication()
    .AddMcpRateLimiting();

var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Default.PreserveReference(true);
mapsterConfig.Compile();
builder.Services.AddSingleton(mapsterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();

var app = builder.Build();

app.UseMcpHttpPipeline();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live")
}).AllowAnonymous();
app.MapMcp(mcpOptions.Route)
    .RequireAuthorization()
    .RequireRateLimiting(RateLimitingPolicyNames.Expensive);

await app.RunAsync();
