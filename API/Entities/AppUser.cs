using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public DateOnly DateOfBirth { get; set; } // Fecha de Nacimiento
        public string KnownAs { get; set; } // Nombre por el cual el usuario
        // es conocido
        public DateTime Created { get; set; } = DateTime.UtcNow; // fecha y hora de
        // creación del perfil de usuario
        public DateTime LastActive { get; set; } = DateTime.UtcNow; // fecha y hora
        // de la ultima vez que el usuario estuvo activo
        public string Gender { get; set; } // almacena el sexo de la persona
        public string Introduction { get; set; } // introducción personal del usuario
        public string LookingFor { get; set; } // Que busca el usuario en la aplicación
        public string Interests { get; set; } // Intereses del usuario
        public string City { get; set; } // la ciudad del usuario
        public string Country { get; set; } // el país del usuario
        public List<Photo> Photos { get; set; } = new(); // almacena una lista de
        //fotos de usuario que representa un objeto de tipo photos y tambien
        // el objeto photo es otra tabla

        /*public int GetAge()
        {
            return DateOfBirth.CalculateAge();
        }*/

        public List<UserLike> LikedByUsers { get; set; } // almacena una lista de
        // usuarios que le han indicado me gusta a este usuario
        public List<UserLike> LikedUsers { get; set; } // almacena una lista de usuarios
        // que este usuario ha indicado me gusta
        public List<Message> MessagesSent { get; set; } // almacena una lista de
        // mensajes que ha enviado este usuario
        public List<Message> MessagesReceived { get; set; } // almacena una lista
        // de mensajes que ha recibido este usuario
        public ICollection<AppUserRole> UserRoles { get; set; } // almacena una colección
        // de roles asociados a este usuario

    }
}