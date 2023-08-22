namespace API.DTOs
{
    public class LikeDto
    {
        public int Id { get; set; } // id
        public string UserName { get; set; } // usuario
        public int Age { get; set; } // edad
        public string KnownAs { get; set; } // apodo del usuario
        public string PhotoUrl { get; set; } // url de la foto
        public string City { get; set; } // ciudad del usuario
        
    }
}