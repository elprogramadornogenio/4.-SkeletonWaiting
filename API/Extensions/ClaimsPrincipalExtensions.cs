using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // this ClaimsPrincipal user es una extension de cualquier instancia ClaimsPrincipal
        public static string GetUsername(this ClaimsPrincipal user)
        {
            // se obtiene el usuario del usuario registrado en la aplicación
            return user.FindFirst(ClaimTypes.Name)?.Value;
            // Obtiene el valor de JwtRegisteredClaimNames.UniqueName del token
        }

        // this ClaimsPrincipal user es una extension de cualquier instancia ClaimsPrincipal
        public static int GetUserId(this ClaimsPrincipal user) // se obtiene la informacion del usuario
        {
            // se obtiene el ID del usuario que esta guardado en el token
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // Obtiene el valor de JwtRegisteredClaimNames.NameId creado en el token
        }
    }
}