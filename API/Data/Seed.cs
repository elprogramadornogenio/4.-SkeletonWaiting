using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            if(await userManager.Users.AnyAsync()) return; // verifica si éxiste usuarios
            // registrados en la aplicación si se cumple la condición retorna
            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json"); 
            // lee los datos del archivo JSON 
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // independientemente de que los atributos
                // del archivo json sean mayusculas o minusculas si se llaman igual que 
                // las propiedades de la clase AppUser esta toma los valores y los asigna 
                // a las propiedades de la clase.
            };

            var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);
            // deserealiza el archivo json userData de acuerdo con las configuraciones de
            // option y retorna una lista de AppUser

            // Crear una lista de roles en este caso "Member", "Admin", "Moderator"
            var roles = new List<AppRole>
            {
                new AppRole{Name="Member"},
                new AppRole{Name="Admin"},
                new AppRole{Name="Moderator"}
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role); // se crea un rol en la base de datos
            }

            foreach (var user in users)
            {
                user.UserName = user.UserName.ToLower(); // pasa el usuario a minusculas
                await userManager.CreateAsync(user, "Pa$$w0rd"); // crea un usuario
                // con contraseña
                await userManager.AddToRoleAsync(user, "Member"); // crea el usuario con rol
                // de miembro
            }

            var admin = new AppUser
            {
                UserName = "admin" // Se va a crear un usuario administrador
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd"); // se crea un usuario administrador
            await userManager.AddToRolesAsync(admin, new [] {"Admin", "Moderator"}); // admin tiene
            // los permisos de Admin y Moderator
        }
    }
}