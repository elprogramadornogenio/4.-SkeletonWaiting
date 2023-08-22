using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext context; // inyecta el contexto de la aplicación

        public LikesRepository(DataContext context)
        {
            this.context = context;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        // recibe como parámetro el id del usuario que dio like y quien lo recibe respecivamente
        {
            return await context.Likes.FindAsync(sourceUserId, targetUserId);
            /*
                Busca un registro de like con los dos parametros recuerde importa el orden
            */
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = context.Users.OrderBy(u => u.UserName).AsQueryable();
            // crea una consulta en base a los usuarios mediante AsQueryable()
            var likes = context.Likes.AsQueryable();
            // crea una consulta en base a la tabla de likes

            if(likesParams.Predicate == "liked") 
            // si el predicado es "liked" significa que va a traer los usuarios que le
            // gusta al usuario en sesión
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                // agrega el filtro de where en la consulta de likes para traer los likes
                // que ha dado el usuario en sesión
                users = likes.Select(like => like.TargetUser);
                // selecciona el usuario que se le ha dado me gusta para traer los usuario
                // que se le han dado "me gusta"
            }

            if(likesParams.Predicate == "likedBy")
            // si el predicado es "likedBy" significa que va a traer los usuarios que le han
            // dado gusta al usuario en sesión
            {
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                // agrega el filtro de where en la consulta de likes para traer los likes
                // que ha los otros usuarios al usuario en sesión
                users = likes.Select(like => like.SourceUser);
                // selecciona el usuario que se le ha dado me gusta para traer los usuario
                // que le han dado "me gusta" al usuario en sesión
            }

            var likedUsers = users.Select(user => new LikeDto 
            {
                UserName = user.UserName, // usuario
                KnownAs = user.KnownAs, // conocido como
                Age = user.DateOfBirth.CalculateAge(), // devuelve la edad
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url, // devuelve la imagen principal
                City = user.City, // ciudad
                Id = user.Id // el id del usuario
            });

            return await PagedList<LikeDto>.CreateAsync(
                likedUsers,  // retorna lista de usuarios
                likesParams.PageNumber, 
                likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await context.Users
                .Include(x => x.LikedUsers) // incluye la lista de usuarios que este usuario
                // ha dado me gusta
                .FirstOrDefaultAsync(x => x.Id == userId); // trae el usuario con el id
        }
    }
}