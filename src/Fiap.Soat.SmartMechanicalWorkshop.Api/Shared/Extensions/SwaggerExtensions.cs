using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            string[] xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (string xmlFile in xmlFiles)
            {
                options.IncludeXmlComments(xmlFile, true);
            }

            options.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "SmartMechanicalWorkshop",
                    Version = "v1",
                    Description =
                        "Veja a documentação no [ReDoc](/docs) <br> Repositorio do projeto: [GitHub](https://github.com/igortessaro/fiap-soat-oficina-mecanica)"
                });
            options.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT no formato: Bearer {seu token}"
                });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, [] }
            });
        });
        return services;
    }
}
