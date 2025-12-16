using InfraFlowSculptor.Api;
using InfraFlowSculptor.Api.Controllers;
using InfraFlowSculptor.Application;
using InfraFlowSculptor.Infrastructure;
using InfraFlowSculptor.Infrastructure.Factories;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Services;
using InfraFlowSculptor.ServiceDefaults;
using Scalar.AspNetCore;
using Shared.Api.Configuration;
using Shared.Api.Errors;
using Shared.Api.Options;
using Shared.Api.RateLimiting;

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
    .AddInfrastructure(builder.Configuration)
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

AuthBearerTokenFactory.SetBearerTokenGetterFunc(cancellationToken =>
{
    var client = app.Services.GetRequiredService<IBearerTokenService>();
    return client.GetBearerTokenAsync(cancellationToken);
});

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
app.UseInfrastructureConfigController();
app.UseKeyVaultControllerController();
app.UseResourceGroupController();
app.MapDefaultEndpoints();

await app.RunAsync();