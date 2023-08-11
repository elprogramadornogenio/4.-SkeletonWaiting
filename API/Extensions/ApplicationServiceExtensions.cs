using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Service;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        // builder.Services.AddApplicationServices(builder.Configuration);
        // Esta funcion se encarga de definir algunos servicios de la aplicación
        // Conexión a bases de datos
        // Contratos entre interfaces y clases
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, 
        IConfiguration config)
        // esta función recibe como parametros una instancia de tipo IServiceCollection
        // Argumento IConfiguration para acceder al archivo appsettings.json
        {
            
            services.AddDbContext<DataContext>(opt => 
            {
                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            services.AddCors(); // habilitar la politica de CORS en la aplicación

            // hay tres métodos para registrar en el contenedor de Injección de dependencias
            // AddTransient. AddScoped, AddSingleton
            services.AddScoped<ITokenService, TokenService>(); // Se registra la interfaz
            // ITokenService y TokenService con el método AddScope, recurdar la analogía de
            // la cafeteria (aplicación web) y el cliente (Peticion HTTP), cada vez que
            // ingresa un cliente se le asigna un camarero (una instancia de tokenService)
            // hasta que un cliente (http) se retira, cada cliente tiene un camarero es
            // decir cada peticion tiene su propia instancia. 
            services.AddScoped<IUserRepository, UserRepository>(); // registrar IUserRepository
            // con la implementación UserRepository mediante el ciclo de vida de la instancia
            // addScoped
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            // inicializar la capacidad de utilizar automapper facilita la transformación
            // de objetos DTO a entities o viserversa
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            // Los atributos de la clase CloudinarySettings van a tener la configuración
            // del archivo appsettings.json de CloudinarySettings pero los nombres
            // de los atributos deben ser iguales a los del appsettings.json
            services.AddScoped<IphotoService, PhotoService>();
            // registra IphotoService con PhotoService para tener un contrato de acuerdo
            // con el ciclo de vida de la instancia con el método AddScoped
            services.AddScoped<ILikesRepository, LikesRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<LogUserActivity>();
            services.AddSignalR(); // registra signalR en la aplicación
            services.AddSingleton<PresenceTracker>(); // se proporciona una unica instancia
            // de la clase PresenceTracker recordando la analogía de la cafeteria (aplicación web)
            // y el cliente (Peticion web), cada vez que ingresa los atendera un camarero
            // (instancia de PresenceTracker), después ingresa otro cliente y el mismo camarero
            // que atendio al anterior cliente va a atender al cliente que acaba de ingresar
            // es decir el mismo camarero va a atender a todos los clientes
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services; // retorna la configuración de todos los servicios configurados 
            // anteriormente.
        }
    }
}