using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next; // hace referencia al siguiente middleware
        private readonly ILogger<ExceptionMiddleware> logger; // es una instancia para registrar
        // eventos
        private readonly IHostEnvironment env; // representa el entorno es decir desarrollo 
        //o produccion

        // se inyectan tres parámetros
        public ExceptionMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionMiddleware> logger, 
            IHostEnvironment env
            )
        {
            this.next = next;
            this.logger = logger;
            this.env = env;
        }

        // el metodo InvokeAsync es el núcleo del middleware
        // cada vez que se recibe una solicitud HTTP
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context); // va a enviar el siguiente middleware de la cadena
            }
            catch (Exception ex) 
            // Si se produce un error en la solicitud del siguiente middleware
            {
                logger.LogError(ex, ex.Message); // se registra el error en el logger
                context.Response.ContentType = "application/json"; // se establece una respuesta
                // de tipo JSON
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                // se establece un código de respuesta status 500

                var response = env.IsDevelopment() // si se encuentra en modo desarrollo
                    ? new ApiException(
                        context.Response.StatusCode, 
                        ex.Message, 
                        ex.StackTrace?.ToString())
                        : new ApiException( // si se encuentra en entorno de producción
                        context.Response.StatusCode, 
                        ex.Message, 
                        "Internal Server Error");
                
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // la primera letra
                    // de las propiedades de la respuesta es en minuscula
                    // ejemplo PrimerNombre = primerNombre
                    // Pascal case todas las primeras letras son en mayuscula
                    // ejemplo PrimerNombre = PrimerNombre
                };

                var json = JsonSerializer.Serialize(response, options); // se utiliza JsonSerializer
                // para convertir la respuesta en JSON con la configuracion de CamelCase configurada
                // en la parte superior

                await context.Response.WriteAsync(json); // se enviará la respuesta al cliente en JSON
            }
        }
    }
}