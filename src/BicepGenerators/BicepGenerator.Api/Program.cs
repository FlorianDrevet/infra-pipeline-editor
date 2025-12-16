using Scalar.AspNetCore;
using BicepGenerator.Api;
using BicepGenerator.Api.Controllers;
using BicepGenerator.Application;
using BicepGenerator.Infrastructure;
using Shared.Api.Configuration;
using Shared.Api.Errors;
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

builder.Services
    .AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddRateLimiting();

var app = builder.Build();

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
app.UseBicepGenerationControllerController();

await app.RunAsync();