namespace API.Errors
{
    public class ApiException // define la estructura de la respuesta de la excepción
    {
        public ApiException(int statusCode, string message, string details)
        {
            StatusCode = statusCode; // guarda el status
            Message = message; // guarda el mensaje de error
            Details = details; // muestra los detalles del error
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}