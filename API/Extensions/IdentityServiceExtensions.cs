using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, 
        IConfiguration config)
        {
            // Configura el sistema de autenticación y autorización para trabajar con la clase
            // AppUser
            services.AddIdentityCore<AppUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false; // el campo establece que la contraseña
                // no requiere campos no alfanumericos como por ejemplo @, #, !, $
            })
            .AddRoles<AppRole>() // agregar la capacidad de utilizar roles de usuario con la clase
            // AppRole
            .AddRoleManager<RoleManager<AppRole>>() // agregar un administrador de roles personalizados
            .AddEntityFrameworkStores<DataContext>(); // Configura el almacenamiento de usuarios
            // roles utilizando EnityFramework
            
            // Configuracion de Autenticación con JWT

            // El elemento de autenticación se ha de llamar cuando pase por el middleware
            // de autenticación por token
            // las peticiones son interceptadas mediante este middleware antes de llegar
            // a los controladores
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            // JwtBearerDefaults.AuthenticationScheme permite la autenticación de usuarios
            // por JWT
            .AddJwtBearer(options => {
                // validar los parametros del token
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // verificar la clave de la firma del emisor
                    // en otras palabras valida que el token no se ha modificado
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(config["TokenKey"])), // accede a las opciones de appsettings.json
                    // y recoge la clave para decodificar el token
                    ValidateIssuer = false, // validar el emisor del token en este caso es falso
                    ValidateAudience = false // validar el receptor del token este caso es falso
                };

                // Es un evento personalizado que se ejecuta al momento de recibir una solicitud
                // esto es cuando se implementa en SignalR
                options.Events = new JwtBearerEvents
                    {
                        // Se ejecuta OnMessageReceived cuando se ha de recibir una solicitud
                        OnMessageReceived = context => 
                        {
                            var accessToken = context.Request.Query["access_token"];
                            // se ha de obtener el token de acceso

                            var path = context.HttpContext.Request.Path; // se obtiene la ruta de 
                            // solicitud actual que ha recibido la solicitud

                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            // verifica que el token exista y que la ruta tenga /hub en la solicitud
                            {
                                context.Token = accessToken; // Si cumple la condicion en el contexto
                                // de la aplicación guarda el token, que permitira almacenar el token
                                // en el contexto autenticación para autenticar la solicitud
                            }

                            return Task.CompletedTask; // Se devuelve la tarea completada e indica que la
                            // solicitud se ha completado
                        }
                    };
                
            });
            // definir reglas de quien tiene acceso a la información
            // cuando se utiliza el atributo [Authorize] en el controlador se verifican los
            // roles
            services.AddAuthorization(opt => 
            {
                opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                // agrega una politica RequireAdminRole que dice que requiere el Rol Admin
                opt.AddPolicy("ModeratePhotoRole", policy => policy
                    .RequireRole("Admin", "Moderator"));
                // agrega una politica ModeratePhotoRole que dice que requiere el Rol Admin
                // y el rol Moderator
            });

            return services;
        }
    }
}