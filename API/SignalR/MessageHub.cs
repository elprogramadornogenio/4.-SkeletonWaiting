using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub: Hub
    {
        private readonly IUnitOfWork unitOfWork; // se inyecta la unidad de trabajo
        private readonly IMapper mapper; // se inyecta funcionalidades de mapper
        private readonly IHubContext<PresenceHub> presenceHub; 
        // se inyecta las funcionalidades de presenseHub 

        public MessageHub(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<PresenceHub> presenceHub
            )
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.presenceHub = presenceHub;
        }


        /*
            Este metodo se ejecuta automaticamente cuando un cliente de tipo
            signalR se conecta al servidor
        */
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext(); //obtiene el contexto actual del http
            //que esta conectado
            var otherUser = httpContext.Request.Query["user"]; // es para obtener el valor
            // del parámetro de la consulta user que es el usuario que se le quiere enviar
            // el mensaje
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            // se inserta el usuario que se encuentra el el contexto actual y el usuario
            // que se le enviará el mensaje
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            /*
                el usuario se conecta mediante su id al groupName n signalR
            */
            var group = await AddToGroup(groupName); // agrega el grupo a base de datos con las
            // los ids de las conexiones de los usuarios en conexión para mensaje

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
            /*
                se envia un mensaje a todos los clientes en el grupo donde los id de conexión
                se encuentren registrados en el grupo
            */

            var messages = await unitOfWork.MessageRepository.GetMessageThread(
                Context.User.GetUsername(), otherUser);
                /*
                    guardar el mensaje del usuario en sesion con el otro usuario
                */

            if(unitOfWork.HasChanges()) await unitOfWork.Complete();
            /*
                guardar los cambios incluso los de otros contextos en bases de datos
            */

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
            /*
                Clients: Es una función de signalR permite enviar datos a clientes especificos
                o grupos de clientes que han sido registrados en
                
                Caller: Es el cliente que está en sesión o en contexto

                SendAsync es un metodo que envia asincronamente el mensaje messages
            */
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            // este metodo se utiliza para organizar en orden alfabetico el nombre del grupo
            // verdadero: {caller}-{other} < 0
            // falso: {other}-{caller} < 0 es decir es mayor que 0
            return stringCompare ? $"{caller}-{other}":$"{other}-{caller}";
        }

        /*
            OnDisconnectedAsync se ejecuta cuando un cliente se desconecta
        */
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup(); // remueve el id de la conexión
            await Clients.Group(group.Name).SendAsync("UpdatedGroup"); // envia al grupo 
            /*
                Clients: Es una función de signalR permite enviar datos a clientes especificos
                o grupos de clientes que han sido registrados
                Group: Envia el mensaje a todos los clientes con sus ids registrados en el mismo 
                nombre del grupo 
            */
            await base.OnDisconnectedAsync(exception); 
            // completa el proceso de desconexión del usuario
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send messages to yourself");
            
            var sender = await unitOfWork.UserRepository
                .GetUserByUsernameAsync(username); // traer los datos 
                // del usuario que va a enviar el mensaje
                // es decir es el emisor del mensaje

            var recipient = await unitOfWork.UserRepository
                .GetUserByUsernameAsync(createMessageDto.RecipientUsername) 
                ?? throw new HubException("Not found user"); // traer los datos
                // del usuario que va a recibir el mensaje
                // es decir el receptor del mensaje
                
            var message = new Message
            {
                Sender = sender, // datos del usuario que envia el mensaje (emisor)
                Recipient = recipient, // datos del usuario que recibe el mensaje (receptor)
                SenderUsername = sender.UserName, // usuario (emisor)
                RecipientUsername = recipient.UserName, // usuario (receptor)
                Content = createMessageDto.Content // contenido del mensaje
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            // se encarga de crear un grupo con los nombres de usuario en orden alfabetico
            // ejemplo lisa-todd pero nunca va a crear un grupo todd-lisa

            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
            // busca si existe el nombre del grupo es decir, si existe por ejemplo lisa-todd

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow; // colocar los mensajes leidos
            }
            else 
            {
                var connections = await PresenceTracker.GetConnectionForUser(recipient.UserName);
                // traer todas las conexiones del usuario que recibe los mensajes (receptor)

                if(connections != null)
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
                    new 
                    {
                        username = sender.UserName,
                        knownAs = sender.KnownAs
                    });
                    // Recibe la variable conexión que contiene todas las conexiones del usuario receptor
                    // SendAsync: Metodo de SignalR para enviar mediante "NewMessagesReceived" 
                    // un objeto con los datos username, knownAs
                }
            }

            unitOfWork.MessageRepository.AddMessage(message); 
            // agregar un mensaje en el repositorio

            if(await unitOfWork.Complete()) 
            {
                await Clients.Group(groupName)
                    .SendAsync("NewMessage", mapper.Map<MessageDto>(message)); // se envia el mensaje 
                    // al grupo especificado con NewMessage
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        /*
            AddToGroup se crea un grupo de conexión
        */
        {
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
            // trae el nombre del grupo de la conexión

            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            // se hace una nueva conexión con el id de la conexion y el nombre del usuario

            if(group == null) // si el grupo no existe
            {
                group = new Group(groupName); // crea un grupo
                unitOfWork.MessageRepository.AddGroup(group); // agrega un grupo a base de datos
            }

            group.Connections.Add(connection); // al grupo se agrega una conexión

            if( await unitOfWork.Complete()) return group; // si se ha completado exitosamente
            // se crea el grupo con la conexión a la base de datos y devuelve el grupo

            throw new HubException("Failed to add to group"); // de lo contrario crea una excepción
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.MessageRepository.
                GetGroupForConnection(Context.ConnectionId); 
            // traer el grupo de acuerdo al id de la conexión

            var connection = group.Connections
                .FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            // trae la conexión id del usuario que esta en sesión o conexión

            unitOfWork.MessageRepository.RemoveConnection(connection); // elimina esa conexión

            if(await unitOfWork.Complete()) return group; 
            // si se ha eliminado la conexión retorna el grupo

            throw new HubException("Failed to remove from group");
            // retorna una excepción

        }
    }
}