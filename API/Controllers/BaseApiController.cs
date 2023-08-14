using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))] // Cada vez que un usuario hace una peticion a los
    // controladores se va a registrar su ultima actividad en base de datos
    [ApiController] // maneja el enrutamiento de datos, manejo de modelos de datos
    // respuestas automáticas en formato JSON
    [Route("api/[controller]")] // este atributo establece la ruta base de los controladores
    // o los endpoints de la aplicación
    public class BaseApiController : ControllerBase
    {
         
    }
}