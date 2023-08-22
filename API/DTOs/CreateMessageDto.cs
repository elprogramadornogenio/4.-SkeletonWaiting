namespace API.DTOs
{
    public class CreateMessageDto
    {
        public string RecipientUsername { get; set; } // usuario a la que se envia al mensaje
        public string Content { get; set; } // contenido del mensaje
    }
}