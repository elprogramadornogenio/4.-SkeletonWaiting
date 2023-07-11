using Microsoft.OpenApi.Models;

namespace API.Extensions
{
    public static class SwaggerAuthenticationServiceExtensions
    {
        public static IServiceCollection AddSwaggerAuthenticationServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>{
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Here Enter JWT with bearer format like bearer token"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new String[] {}
                }
            });
        });
            return services;
        }
    }
}