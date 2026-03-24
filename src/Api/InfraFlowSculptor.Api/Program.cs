using InfraFlowSculptor.Api;
using InfraFlowSculptor.Api.Controllers;
using InfraFlowSculptor.Application;
using InfraFlowSculptor.Infrastructure;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.ServiceDefaults;
using InfraFlowSculptor.Api.Configuration;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Api.Options;
using InfraFlowSculptor.Api.RateLimiting;

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

builder.AddNpgsqlDataSource("infraDb");
builder.AddNpgsqlDbContext<ProjectDbContext>(connectionName: "infraDb");

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

builder.AddServiceDefaults();

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
app.UseContainerAppEnvironmentController();
app.UseContainerAppController();
app.UseLogAnalyticsWorkspaceController();
app.UseApplicationInsightsController();
app.UseCosmosDbController();
app.UseSqlServerController();
app.UseSqlDatabaseController();
app.UseAppSettingController();
app.UseBicepGenerationController();
app.MapDefaultEndpoints();

await app.RunAsync();