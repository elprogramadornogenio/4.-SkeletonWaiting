using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    public class AccountController: BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        // Contructor inyecta tres dependencias UserManager, ITokenService, IMapper
        public AccountController(
            UserManager<AppUser> userManager, 
            ITokenService tokenService,
            IMapper mapper
            )
        {
            this.userManager = userManager;
            this._tokenService = tokenService;
            this._mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            // verificar que el usuario existe si existe envia una respuesta 404
            if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");
            // si no se cumple la condicion anterior transforma RegisterDto a AppUser
            // mediante el mapper
            var user = _mapper.Map<AppUser>(registerDto);
            
            // userName es igual al usuario en minuscula
            user.UserName = registerDto.Username.ToLower();
            
            // se va a crear un usuario con la contraseña registrada en RegisterDto
            var result = await userManager.CreateAsync(user, registerDto.Password);

            // si no fue exitoso la creacion del usuario envia una respuesta 404
            if(!result.Succeeded) return BadRequest(result.Errors);

            // crea el rol de usuario a user con "Member"
            var roleResult = await userManager.AddToRoleAsync(user, "Member");

            // si la creacion del rol no fue exitoso envia una respuesta 404
            if(!roleResult.Succeeded) return BadRequest(result.Errors);

            // si la creacion del usuario fue éxitoso crea un usuario Dto
            return new UserDto 
            {
                Username = user.UserName, // enviar el usuario
                Token = await _tokenService.CreateToken(user), // enviar token
                KnownAs = user.KnownAs, // enviar el apodo
                Gender = user.Gender // enviar el sexo
            };
            
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // se busca usuario y se incluye las propiedades asociadas como fotos
            // se utiliza SingleOrDefaultAsync para traer el unico usuario que éxiste
            var user = await userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => 
            x.UserName == loginDto.Username);


            // si usuario es nulo quiere decir que no existe y regresa una respuesta 401
            if(user == null) return Unauthorized("invalid username");

            // si existe el usuario compara la contraseña con la que existe en base de datos
            var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

            // si el resultado es falso lo convierte en verdadero y retorna una respuesta 401
            if(!result) return Unauthorized("Invalid password");


            // retorna los datos de usuario
            return new UserDto 
            {
                Username = user.UserName, //  usuario
                Token = await _tokenService.CreateToken(user), // crear token
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url, // retorna url foto principal
                KnownAs = user.KnownAs, // apodo
                Gender = user.Gender // sexo
            };
        }

        private async Task<bool> UserExists(string username)
        {
            // verifica si el usuario existe en toda la tabla Users
            return await userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}