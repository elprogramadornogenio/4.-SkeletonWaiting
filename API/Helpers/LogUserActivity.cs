using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        // el objetivo de este código es registrar el campo LastActive para
        // guardar el campo de ultima actividad registrada del usuario
        // El código se ejecuta cuando la acción del controlador
        // se está ejecutando
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context, // contiene la información del contexto
            ActionExecutionDelegate next // representa la siguiente acción
            )
        {
            var resultContext = await next(); // se invoca la o almacena la siguiente
            // accion en resultContext

            if(!resultContext.HttpContext.User.Identity.IsAuthenticated) return;
            // si el usuario no está autenticado no realiza ninguna acción

            var userId = resultContext.HttpContext.User.GetUserId();
            // el usuario id se obtiene por la extension de la instancia ClaimsPrincipal

            var unitOfWork = resultContext.HttpContext.RequestServices
                .GetRequiredService<IUnitOfWork>();
            // se obtiene la instancia de la unidad de trabajo
            
            var user = await unitOfWork.UserRepository.GetUserByIdAsync(userId);
            // obtener el usuario por Id

            user.LastActive = DateTime.UtcNow; // se actualiza la hora y la fecha
            // actual con el campo LastActive

            await unitOfWork.Complete(); // Se actualiza la información en la base de datos
        }
    }
}