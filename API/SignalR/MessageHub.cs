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
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IHubContext<PresenceHub> presenceHub;

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

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await unitOfWork.MessageRepository.GetMessageThread(
                Context.User.GetUsername(), otherUser);

            if(unitOfWork.HasChanges()) await unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}":$"{other}-{caller}";
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
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
        {
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);

            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if(group == null)
            {
                group = new Group(groupName);
                unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if( await unitOfWork.Complete()) return group;

            throw new HubException("Failed to add to group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);

            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            unitOfWork.MessageRepository.RemoveConnection(connection);

            if(await unitOfWork.Complete()) return group;

            throw new HubException("Failed to remove from group");

        }
    }
}