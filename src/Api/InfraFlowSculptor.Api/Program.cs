using InfraFlowSculptor.Api;
using InfraFlowSculptor.Api.Common;
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

// Audit SEC-002 (2026-04-23): security headers applied to every response.
// CSP intentionally omitted: see SEC-002 follow-up.
app.Use(async (ctx, next) =>
{
    var headers = ctx.Response.Headers;
    headers["X-Frame-Options"] = "DENY";
    headers["X-Content-Type-Options"] = "nosniff";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter(); //After UseRouting
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserProvisioningMiddleware>();

//Controllers
app.UseProjectController();
app.UseInfrastructureConfigController();
app.UseNamingTemplateController();
app.UseKeyVaultControllerController();
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
app.UseImportController();
app.UseBicepGenerationController();
app.UsePipelineGenerationController();

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});

await app.RunAsync();