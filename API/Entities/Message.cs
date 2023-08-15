namespace API.Entities
{
    public class Message
    {
        public int Id { get; set; } // el id único de cada mensaje
        public int SenderId { get; set; } // el id del usuario que envio el mensaje
        public string SenderUsername { get; set; } // el nombre del usuario que envio el mensaje
        public AppUser Sender { get; set; } // la propiedad del usuario que ha enviado el mensaje
        public int RecipientId { get; set; } // el id del usuario que ha recibido el mensaje
        public string RecipientUsername { get; set; } 
        //el nombre del usuario que ha recibido el mensaje
        public AppUser Recipient { get; set; } // la propiedad del usuario que recibio el mensaje
        public string Content { get; set; } // el contenido del mensaje
        public DateTime? DateRead { get; set; } 
        // la fecha y hora que el destinatario leyó el mensaje
        public DateTime MessageSent { get; set; } = DateTime.UtcNow; 
        // fecha y hora que se envio el mensaje
        public bool SenderDeleted { get; set; } // Un indicador que muestra si el remitente 
        // (Usuario que envio el mensaje) ha eliminado el mensaje
        public bool RecipientDeleted { get; set; } // Un indicador que muestra si el remitente 
        // (Usuario que recibe el mensaje) ha eliminado el mensaje 
        
    }
}