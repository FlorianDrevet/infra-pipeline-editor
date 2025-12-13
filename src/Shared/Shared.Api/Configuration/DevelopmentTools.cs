using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Shared.Api.Options;

namespace Shared.Api.Configuration;

public static class DevelopmentTools
{
    public static void AddDevelopmentTools(this WebApplication app, IConfiguration configuration)
    {
        if (!app.Environment.IsDevelopment()) 
            return;
        
        app.UseDeveloperExceptionPage();
        app.MapOpenApi().AllowAnonymous();
    
        var scalarOauthConfiguration = configuration
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
}