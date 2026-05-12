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
    // Audit SEC-005 (2026-05-12): explicit allow-list of methods, headers, and origins.
    // Origins overridable via configuration key "Cors:AllowedOrigins" (string[]) for non-dev environments.
    var configuredOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>();
    var origins = configuredOrigins is { Length: > 0 }
        ? configuredOrigins
        : new[] { "http://localhost:4200" };

    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins(origins)
            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
            .WithHeaders("Content-Type", "Authorization", "Accept", "X-Requested-With")
            .AllowCredentials();
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

// Audit SEC-002 (2026-05-12): security headers applied to every response.
// Includes a strict default-deny CSP suitable for a JSON API (no inline scripts, no embedding).
app.Use(async (ctx, next) =>
{
    var headers = ctx.Response.Headers;
    headers["X-Frame-Options"] = "DENY";
    headers["X-Content-Type-Options"] = "nosniff";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    headers["Cross-Origin-Opener-Policy"] = "same-origin";
    headers["Cross-Origin-Resource-Policy"] = "same-site";
    // CSP tailored for a JSON API: no resources, no embedding. The frontend serves its own assets.
    headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";
    // X-XSS-Protection intentionally NOT set: deprecated and can introduce vulnerabilities in legacy browsers (OWASP guidance).
    await next();
});

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