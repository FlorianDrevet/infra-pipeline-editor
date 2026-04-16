using InfraFlowSculptor.Api;
using InfraFlowSculptor.Api.Controllers;
using InfraFlowSculptor.Application;
using InfraFlowSculptor.Infrastructure;
using InfraFlowSculptor.Api.Configuration;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Api.Options;
using InfraFlowSculptor.Api.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins(
            "http://localhost:4200"
        );
    });
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("IsAdmin", policy => policy.RequireRole("Admin")); 

builder.Services
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
app.UseCors("CorsPolicy");

app.UseErrorHandling();
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter(); //After UseRouting
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

//Endpoints
app.MapProjectEndpoints();
app.MapInfrastructureConfigEndpoints();
app.MapNamingTemplateEndpoints();
app.MapKeyVaultEndpoints();
app.MapResourceGroupEndpoints();
app.MapRedisCacheEndpoints();
app.MapRoleAssignmentEndpoints();
app.MapStorageAccountEndpoints();
app.MapAppServicePlanEndpoints();
app.MapWebAppEndpoints();
app.MapFunctionAppEndpoints();
app.MapUserAssignedIdentityEndpoints();
app.MapAppConfigurationEndpoints();
app.MapAppConfigurationKeyEndpoints();
app.MapContainerAppEnvironmentEndpoints();
app.MapContainerAppEndpoints();
app.MapLogAnalyticsWorkspaceEndpoints();
app.MapApplicationInsightsEndpoints();
app.MapCosmosDbEndpoints();
app.MapSqlServerEndpoints();
app.MapSqlDatabaseEndpoints();
app.MapServiceBusNamespaceEndpoints();
app.MapContainerRegistryEndpoints();
app.MapEventHubNamespaceEndpoints();
app.MapAppSettingEndpoints();
app.MapBicepGenerationEndpoints();
app.MapPipelineGenerationEndpoints();

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});

await app.RunAsync();