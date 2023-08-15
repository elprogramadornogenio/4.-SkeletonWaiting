namespace API.Entities
{
    public class Connection
    {
        public Connection() 
        {
            
        }
        public Connection(string connectionId, string username)
        // guarda conexión de SignalR (id conexión, usuario registrado)
        {
            ConnectionId = connectionId;
            Username = username;    
        }
        public string ConnectionId { get; set; } // Id conexión
        public string Username { get; set; } // nombre del usuario
    }
}