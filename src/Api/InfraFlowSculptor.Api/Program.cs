using InfraFlowSculptor.Api.Common.RateLimiting;
using InfraFlowSculptor.Api.Configuration;
using InfraFlowSculptor.Api.Controllers;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Api.Options;
using InfraFlowSculptor.Application;
using InfraFlowSculptor.Infrastructure;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.ServiceDefaults;
using Scalar.AspNetCore;

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi().AllowAnonymous();
    
    var scalarOauthConfiguration = builder.Configuration
        .GetSection(ScalarOAuthOptions.SectionName)
        .Get<ScalarOAuthOptions>();
    
    app.MapScalarApiReference(options =>
    {
        options.Layout = ScalarLayout.Classic;
        if (scalarOauthConfiguration is not null)
        {
            options.AddPreferredSecuritySchemes("OAuth2")
                .AddImplicitFlow("OAuth2", flow =>
                {
                    flow.ClientId = scalarOauthConfiguration.ClientId;
                    flow.SelectedScopes = scalarOauthConfiguration.Scopes.Select(s => $"{scalarOauthConfiguration.Audience}/{s}").ToArray();
                    flow.AuthorizationUrl = scalarOauthConfiguration.AuthorizationUrl;
                });
        }
    }).AllowAnonymous();
}

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

app.Run();