using InfraFlowSculptor.Api;
using InfraFlowSculptor.Api.Common;
using InfraFlowSculptor.Api.Controllers;
using InfraFlowSculptor.Application;
using InfraFlowSculptor.Infrastructure;
using InfraFlowSculptor.WebDefaults.Security;
using InfraFlowSculptor.Api.Configuration;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Api.Options;
using InfraFlowSculptor.Api.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddApiRequestLimits(builder.Configuration);

builder.Services
    .AddApiAuthorization()
    .AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddRateLimiting();

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddOptions<ScalarOAuthOptions>()
        .Bind(builder.Configuration.GetSection(ScalarOAuthOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.AddDevelopmentTools(builder.Configuration);

//Middleware
app.UseCors();

app.UseErrorHandling();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter(); // After UseRouting and authentication so user-based partitions can resolve claims.
app.UseStatusCodePages();
app.UseAuthorization();
app.UseMiddleware<UserProvisioningMiddleware>();

//Controllers
app.UseProjectController();
app.UseInfrastructureConfigController();
app.UseNamingTemplateController();
app.UseKeyVaultController();
app.UseResourceGroupController();
app.UseRedisCacheController();
app.UseRoleAssignmentController();
app.UseStorageAccountController();
app.UseAppServicePlanController();
app.UseWebAppController();
app.UseFunctionAppController();
app.UseUserAssignedIdentityController();
app.UseAppConfigurationController();
app.UseAppConfigurationKeyController();
app.UseContainerAppEnvironmentController();
app.UseContainerAppController();
app.UseLogAnalyticsWorkspaceController();
app.UseApplicationInsightsController();
app.UseCosmosDbController();
app.UseSqlServerController();
app.UseSqlDatabaseController();
app.UseServiceBusNamespaceController();
app.UseContainerRegistryController();
app.UseEventHubNamespaceController();
app.UsePersonalAccessTokenController();
app.UseAppSettingController();
app.UseSecureParameterMappingController();
app.UseCustomDomainController();
app.UseVirtualNetworkController();
app.UseNetworkSecurityGroupController();
app.UsePrivateDnsZoneController();
app.UseFrontDoorController();
app.UsePrivateEndpointController();
app.UseImportController();
app.UseBicepGenerationController();
app.UsePipelineGenerationController();

// Health checks
app.MapApiHealthChecks();

await app.RunAsync();