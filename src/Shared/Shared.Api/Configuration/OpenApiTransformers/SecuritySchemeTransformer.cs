using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Api.Configuration.OpenApiTransformers;

public sealed class SecuritySchemeTransformer : IOpenApiDocumentTransformer
{
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
