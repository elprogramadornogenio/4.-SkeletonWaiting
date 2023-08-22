using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Service
{
    public class TokenService : ITokenService
    {
        public readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> userManager;

        public TokenService(
            IConfiguration configuration, 
            // se utiliza IConfiguration para acceder a appsettings.json
            UserManager<AppUser> userManager
            // se utiliza para adquirir los datos del usuario 
            )
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenKey"]));
            // matriz de bytes para guardar la clave secreta
            this.userManager = userManager;
        }
        public async Task<string> CreateToken(AppUser user)
        // funcionalidad de Crear Token recibir como 
        {
            /*
                los claims o reclamaciones son piezas de información del usuario
            */
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()), 
                // guarda el identificador único del usuario
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                // guarda el nombre unico del usuario
            };

            var roles = await userManager.GetRolesAsync(user); // traer los roles del usuario

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            // se utiliza para agregar los roles del usuario en el token mediante AddRange

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            // se va a crear la firma del token que recibe la clave secreta y el algoritmo de firma


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // se agregan las reclamaciones
                Expires = DateTime.Now.AddDays(7), // el token expira en 7 dias
                SigningCredentials = creds // agrega la fimra
            };

            var tokenHandler = new JwtSecurityTokenHandler(); // se utiliza se objeto
            // para crear el token real mediante la descripción
            var token = tokenHandler.CreateToken(tokenDescriptor); // crea el token

            return tokenHandler.WriteToken(token); //devuelve el token como cadena mediante WriteToken
        }
    }
}