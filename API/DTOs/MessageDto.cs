namespace API.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; } // id del mensaje
        public int SenderId { get; set; } // id del que ha enviado el mensaje (emisor)
        public string SenderUsername { get; set; } // usuario que ha enviado el mensaje (emisor)
        public string SenderPhotoUrl { get; set; } // foto del usuario que ha enviado el mensaje
        public int RecipientId { get; set; } // id del usuario que recibe el mensaje (receptor)
        public string RecipientUsername { get; set; } // usuario que ha recibido el mensaje (receptor)
        public string RecipientPhotoUrl { get; set; } // foto del usuario que ha recibido el mensaje
        public string Content { get; set; } // contenido del mensaje
        public DateTime? DateRead { get; set; } // fecha y hora de haber leido el mensaje
        public DateTime MessageSent { get; set; } // fecha y hora de envio del mensaje
    }
}