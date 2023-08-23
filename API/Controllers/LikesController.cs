using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController: BaseApiController
    {
        private readonly IUnitOfWork unitOfWork; // inyecta la unidad de trabajo

        public LikesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username) 
        // recibe como parametro de usuario que se le ha dado like 
        {
            var sourceUserId = User.GetUserId(); 
            // se obiene el id del usuario en sesión
            var likedUser = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            // trae los datos del usuario que se le ha dado like
            var sourceUser = await unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);
            // trae los datos del usuario que ha dato like al usuario likedUser
            if(likedUser == null) return NotFound();
            // si el usuario que se le ha dado like no éxiste retorna un status 400

            if(sourceUser.UserName == username) return BadRequest("You cannot like yourself");
            // si el usuario que está en sesión es el mismo que se le ha dado like entonces
            // aparece un error de status 400

            var userLike = await unitOfWork.LikesRepository
                .GetUserLike(sourceUserId, likedUser.Id);
            /*
                Se consulta si ya existe el like de parte del usuario en sesión al usuario
                destino
            */

            if(userLike != null) return BadRequest("You already like this user");
            // si existe el like entonces retorna un status 400

            userLike = new UserLike
            {
                SourceUserId = sourceUserId, 
                // guarda el id de la persona que ha dado like
                TargetUserId = likedUser.Id
                // guarda el id de la persona que ha recibido el like
            };

            sourceUser.LikedUsers.Add(userLike); // guarda el like que ha dado el usuario de origen

            if(await unitOfWork.Complete()) return Ok(); // si se ha guardado correctamente retorna un status 200

            return BadRequest("Failed to like user"); // de lo contrario retorna un status 400
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes(
            [FromQuery]LikesParams likesParams
            )
        {
            likesParams.UserId = User.GetUserId(); 
            // trae el id del usuario en sesión
            var users = await unitOfWork.LikesRepository.GetUserLikes(likesParams);
            // trae los usuarios que tienen un me gusta del usuario en sesión
            Response.AddPaginationHeader(new PaginationHeader(
                users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
            return Ok(users); // retorna un status 200 con la lista de usuarios
        }
    }
}