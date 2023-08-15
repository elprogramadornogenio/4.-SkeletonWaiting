using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; } // representa el id de la foto
        public string Url { get; set; } // representa el Id de la foto
        public bool IsMain { get; set; } // representa si la foto es principal o no
        public string PublicId { get; set; } // representa el Id publico cloudinary
        public int AppUserId { get; set; } // representa el Id del usuario
        public AppUser AppUser { get; set; } // propiedad de navegación del usuario
        // se utiliza para poder navegar al usuario que contiene la foto
    }
}