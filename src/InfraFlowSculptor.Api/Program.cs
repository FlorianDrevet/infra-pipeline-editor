using InfraFlowSculptor.Api;
using InfraFlowSculptor.Api.Common.RateLimiting;
using InfraFlowSculptor.Api.Controllers;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application;
using InfraFlowSculptor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
app.UseAuthenticationController();

app.Run();