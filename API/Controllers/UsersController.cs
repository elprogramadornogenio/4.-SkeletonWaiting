using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize] // mediante esta etiqueta se valida que el usuario está autenticado
    // y tiene los permiso si no cumple los requisitos devuelve status 401 Unauthorized
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper _mapper;
        private readonly IphotoService _photoService;

        public UsersController(
            IUnitOfWork unitOfWork, // inyectar unidad de trabajo
            IMapper mapper, // inyectar mapper
            IphotoService photoService) // inyectar servicio de fotos de cloudinary
        {
            this.unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers(
            [FromQuery]UserParams userParams
            )
        {

            var gender = await unitOfWork.UserRepository
                .GetUserGender(User.GetUsername()); // traer el sexo del usuario actual

            userParams.CurrentUsername = User.GetUsername(); // en userParams guarda el usuario
            // actual

            if(string.IsNullOrEmpty(userParams.Gender)) // si el sexo está null o vacío
            {
                userParams.Gender = gender == "male" ? "female": "male";
            }            

            var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);
            // devuelve lista de usuarios PagedList<MemberDto> users con la páginación
            
            /*
                Agrega una respuesta de tipo cabecera AddPaginationHeader y agrega la
                información de la paginación.
            */
            Response.AddPaginationHeader(new PaginationHeader(
                users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages
            ));

            return Ok(users); // devuelve un status 200 con users
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await unitOfWork.UserRepository.GetMemberAsync(username); 
            // devuelve un único usuario miembro registrado en la base de datos
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            // obtener el usuario que está en sesión

            if(user == null) return NotFound();  // si el usuario no se encuentra devuelve 404

            _mapper.Map(memberUpdateDto, user); 
            /*
                mapea los datos de MemberUpdateDto a AppUser es decir copia los elementos de
                memberUpdateDto a user para actualizar los datos del usuario
            */
            

            if(await unitOfWork.Complete()) return NoContent();
            /*
                quiere decir que los datos se han guardado correctamente y no hay contenido
                que devolver 204 (No Content)
            */ 

            return BadRequest("Failed to update user"); // devuelve un error tipo 400
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            // obtener el usuario que está en sesión
            
            if(user == null) return NotFound(); // si el usuario no se encuentra devuelve 404

            var result = await _photoService.AddPhotoAsync(file); // agrega la foto en cloudinary

            if(result.Error != null) return BadRequest(result.Error.Message);
            // si hay un error devuelve status 400 y el mensaje

            // Crear un objeto de tipo Photo
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri, // guarda la url
                PublicId = result.PublicId // guarda el id publico de la foto
            };

            if(user.Photos.Count == 0) photo.IsMain = true; // si el usuario no tiene fotos
            // esa foto es tomada como principal

            user.Photos.Add(photo); // se guarda la foto en base de datos

            if(await unitOfWork.Complete()) // actualiza la base de datos
            {
                return CreatedAtAction(nameof(GetUser), 
                    new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
                /*
                    CreatedAtAction este método se utiliza  para crear una respuesta HTTP con
                    estatus 201 (Created)
                    nameof es para devolver el nombre de la función o propiedad en este caso
                    devuelve GetUser
                    new {username = user.UserName} crea un objeto anónimo y devuelve la 
                    propiedad username al que se le ha agregado la foto
                    _mapper.Map<PhotoDto>(photo) devuelve la foto creada
                */
            }

            return BadRequest("Problem adding photo"); // si no se ha creado correctamente
            // devuelve 400
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            // obtener el usuario que está en sesión
            if(user == null) return NotFound(); // si el usuario no se encuentra devuelve 404
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId); 
            // devuelve la foto con el id que va a ser la imagen principal
            if(photo == null) return NotFound(); // si la foto no se encuentra devuelve 404
            if(photo.IsMain) return BadRequest("This is already your main photo");
            // si la foto es la principal devolvera 400

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain); 
            // devuelve la foto principal
            if(currentMain != null) currentMain.IsMain = false; // cambia el estado de la foto
            // principal actual a falsa
            photo.IsMain = true; // cambia la foto que desea que sea principal a verdadero

            if(await unitOfWork.Complete()) return NoContent(); // devuelve un status 205
            // quiere decir que los datos se han actualizado correctamente pero no devuelve
            // contenido

            return BadRequest("Problem setting the main photo"); // devuelve un status 400
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId) {

            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            // obtener el usuario que está en sesión

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId); 
            // devuelve la foto con el id
            if(photo == null) return NotFound(); // si no encuentra la foto devuelve status 404
            if(photo.IsMain) return BadRequest("You cannot delete your main photo"); 
            // si la foto es la principal no se puede eliminar y retorna un status 400
            if(photo.PublicId != null) // si la foto tiene una PublicId
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                // elimina la foto en cloudinary
                if(result.Error != null) return BadRequest(result.Error.Message);
                // retorna los errores mediante un status 400
            }
            user.Photos.Remove(photo); // elimina la foto en bases de datos

            if(await unitOfWork.Complete()) return Ok(); 
            // si se actualiza la base de datos correctamente devuelve un status 200
            return BadRequest("Problem deleting photo");
            // de lo contrario un status 400
        }
    }
}