namespace API.Helpers
{
    public class MessageParams: PaginationParams
    {
        public string Username { get; set; } // usuario que quiere recuperar los mensajes
        public string Container { get; set; } = "Unread"; // tipo de contenido
    }
}