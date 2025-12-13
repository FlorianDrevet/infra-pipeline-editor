using InfraFlowSculptor.Api.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace InfraFlowSculptor.Api.Configuration.OpenApiTransformers;

internal sealed class SecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly ScalarOAuthOptions _options;

    public SecuritySchemeTransformer(IOptions<ScalarOAuthOptions> scalarOptions)
    {
        _options = scalarOptions.Value;
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var schemaKey = "OAuth2";
        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            [schemaKey] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Scheme = schemaKey,
            },
        };
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = securitySchemes;
    }
} 
