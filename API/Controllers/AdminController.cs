using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController: BaseApiController
    {
        private readonly UserManager<AppUser> userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        // Policy = RequireAdminRole es decir el permiso de Admin
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            // traer los usuarios con sus roles
            var users = await userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                }).ToListAsync();
            return Ok(users); // retorna un status 200
        }

        [Authorize(Policy = "RequireAdminRole")]
        // Policy = RequireAdminRole es decir el permiso de Admin
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(
            string username, // usuario a cambiar roles
            [FromQuery] string roles // roles de usuario
            )
        {
            if(string.IsNullOrEmpty(roles)) 
                return BadRequest("You must select at least one role");

            var selectedRoles = roles.Split(",").ToArray(); // crea un array separado con comas

            var user = await userManager.FindByNameAsync(username); // traes el usuario

            if(user == null) return NotFound(); // si el usuario no existe retonar un error 404

            var userRoles = await userManager.GetRolesAsync(user); // trae los roles de ese usuario

            var result = await userManager
                .AddToRolesAsync(user, selectedRoles.Except(userRoles));
            // agregar los roles de usuarios de selectedRoles excepto los roles que ya tiene
            // la variable userRoles
            
            if(!result.Succeeded) 
                return BadRequest("Failed to add to roles");
            // si el resultado no fue exitoso devuelve un status 400

            result = await userManager.RemoveFromRolesAsync(user, userRoles
                .Except(selectedRoles));
            // Remueve los demas roles que tenia excepto los selectedRoles

            if(!result.Succeeded) return BadRequest("Failed to remove from roles");
            // si el resultado no fue exitoso devuelve un status 400

            return Ok(await userManager.GetRolesAsync(user)); 
            // trae todos los roles de usuario de ese usuario
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        // Policy = ModeratePhotoRole es decir el permiso de Admin, Moderator
        [HttpGet("photo-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderatiors can see this");
        }
    }
}