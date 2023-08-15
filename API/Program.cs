using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // se crea una instancia de tipo
// WebApplicationBuilder para configurar nuestra aplicación con los servicios

// Agregar los servicios al contenedor de WebApplicationBuilder para construir la
// aplicación web

builder.Services.AddControllers(); // Agregar el servicio para admitir controladores 
// en la aplicación
// Los controladores son EndPoints donde se manejan las peticiones HTTP


builder.Services.AddApplicationServices(builder.Configuration); // adicionar servicios
// de aplicación como ciclos de vida de las instancias de tipo contrato, automappers,
// repositorios.
builder.Services.AddSwaggerAuthenticationServices(); // adicionar configuraciones de 
// autenticacion en Swagger
builder.Services.AddIdentityServices(builder.Configuration); // agregar servicios de 
// autenticación y autorización

// Configuracion de Swagger

builder.Services.AddEndpointsApiExplorer(); // Agregar los Endpoints de la API (Controlador) 
builder.Services.AddSwaggerGen(); // Generar todas las funcionalidades de Swagger

var app = builder.Build(); // Crear una instancia de la aplicación web con la configuración
// de los servicios previamente configurados

app.UseMiddleware<ExceptionMiddleware>();  // Se registra un módulo personalizado de middleware
// con el fin de manejar los errores de la aplicación se ejecuta la clase ExceptionMiddleware
// cada vez que se ejecuta una peticion HTTP

// Configure the HTTP request pipeline.
app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200"));
// la configuracion de los CORS permite la comunicación con otro servidor desde otro origen en
// este caso nuestra aplicación esta en dos puertos diferentes


if (app.Environment.IsDevelopment()) // Si las variables de entorno estan configuradas en desarrollo
{
    app.UseSwagger(); // habilitar el middleware de Swagger
    app.UseSwaggerUI(); // habilitar el middleware de Swagger UI
}

app.UseHttpsRedirection(); // Redirigir el tráfico HTTP a HTTPS para una comunicación segura

app.UseAuthentication(); 
// Habilitar las funcionalidades de Autenticación por ejemplo tomar el token en los encabezados
// de las peticiones HTTP realizar una codificacion y decodificacion, el usuario autenticado
// correctamente se establece en el contexto de la aplicación

app.UseAuthorization();
// Habilitar las funcionalidades de Autorizacion por ejemplo verificar si el usuario tiene los
// permisos para acceder a un recurso, es decir se encarga de habilitar o negar el acceso
// de los usuarios que se autenticaron.

app.MapControllers(); // asignar rutas a los controladores para manejar solicitudes

// Funcionalidades de SignalR

app.MapHub<PresenceHub>("hubs/presence"); // registrar el hub de presence con una ruta especifica 

app.MapHub<MessageHub>("hubs/message"); // registrar el hub message con una ruta especifica

using var scope = app.Services.CreateScope(); // se va a crear un ambito de servicios
// using es para que el ambito se libere despues de su uso
var services = scope.ServiceProvider; // una vez que se crea el ambito de servicios se obtiene
// el proveedor de servicios este es el encargado de proporcionar instancias de servicios
// en toda la aplicación, estas se utilizan especilmente para permisos de usuarios, administración
// de usuarios y migraciones de bases de datos.

try
{
    var context = services.GetRequiredService<DataContext>(); // se obtiene una instancia
    // del contexto de la aplicación para interactuar con el Entity Framework
    var userManager = services.GetRequiredService<UserManager<AppUser>>(); 
    // Se obtiene una instancia del administrador de usuarios, CRUD para usuarios
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    // Se obtiene una instancia de roles de usuario, CRUD para roles
    await context.Database.MigrateAsync(); // aplica migraciones pendientes en la base de datos
    await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]"); // ejecuta una consulta
    // sql directamente a la base de datos para eliminar conexiones
    await Seed.SeedUsers(userManager, roleManager); //
}
catch (Exception ex)
{
    var logger = services.GetService<ILogger<Program>>(); // se crea un logger relacionado con
    // la clase Program
    logger.LogError(ex, "An error occurred during migration"); // cuando ocurra un error
    // en la consola de comandos se dispara el mensaje.
    
}

app.Run(); // Finalimente la aplicación se ejecuta y empieza a escuchar peticiones
// es decir la aplicacion se pone en funcionamiento.
