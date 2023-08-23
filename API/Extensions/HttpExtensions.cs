using System.Text.Json;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(
            this HttpResponse response, // se agrega a los encabezados http
            PaginationHeader header) // agrega la cabecera de la paginación
        {
            // se agrega el objeto JsonSerializerOptions para ver como se serializa
            // el objeto header es decir la primera en mayuscula ejemplo
            //nombreUsuario en vez de NombreUsuario
            var jsonOptions = new JsonSerializerOptions 
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            response.Headers.Add("Pagination", JsonSerializer.Serialize(header, 
                jsonOptions)); // agrega los elementos de la paginación y jsonOptions
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
            // agrega el encabezado Access-Control-Expose-Headers que permite encabezados
            // personalizados para que sean accesibles en javascript o angular
        }
    }
}