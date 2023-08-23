using Microsoft.OpenApi.Models;

namespace API.Extensions
{
    public static class SwaggerAuthenticationServiceExtensions
    {
        public static IServiceCollection AddSwaggerAuthenticationServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>{
            c.SwaggerDoc("v1", new OpenApiInfo // documentacion de swagger
            {
                Title = "API", // titulo de la aplicación en swagger
                Version = "v1" // la versión de la aplicación
            });
            // se agrega una definición de seguridad Bearer que especifica como debe agregar
            // el token con la expresion Bearer Token
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization", // establece el nombre del esquema en la cabecera de la peticion http
                Type = SecuritySchemeType.ApiKey, // se trata del tipo de esquema de seguridad
                // SecuritySchemeType.ApiKey es que el sistema de seguridad esta basado en api (json web token)
                Scheme = "Bearer", // se indica el nombre del esquema utilizado en este caso Bearer un jwt portador
                BearerFormat = "JWT", // se espera un token en formato JSON web token
                In = ParameterLocation.Header, // especifica donde se espera que se proporcione el token
                // en este caso ParameterLocation.Header en la cabecera
                Description = "Here Enter JWT with bearer format like bearer token"
                // la descripción del token
            });
            // agregar en un requisito de seguridad
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    /*
                        Aquí se crea una instancia de OpenApiSecurityScheme, 
                        que representa un esquema de seguridad en Swagger
                    */
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme, // se hace referencia a un esquema de seguridad
                            Id = "Bearer" // hace referencia al esquema AddSecurityDefinition "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });
            return services; // retorna los servicios retornados
        }
    }
}