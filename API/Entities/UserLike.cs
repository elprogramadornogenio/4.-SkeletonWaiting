namespace API.Entities
{
    // esta clase representa una relacion de me gusta con AppUser
    // un usuario fuente (SourceUser) a usuario destino (UserTarget)
    public class UserLike
    {
        public AppUser SourceUser { get; set; } // esta es una propiedad de navegación
        // representa el usuario origen que dio like
        public int SourceUserId { get; set; } // es el id del usuario origen que dio me gusta
        public AppUser TargetUser { get; set; } //esta es una propiedad de navegación
        // del usuario destino que le han dado me gusta
        public int TargetUserId { get; set; } // es el id del usuario destino que le han dado
        // me gusta
    }
}