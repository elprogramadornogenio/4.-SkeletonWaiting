using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context; // inyectar el contexto de la aplicación
        private readonly IMapper mapper; // inyectar mappper

        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            context.Groups.Add(group); 
            // adicionar un grupo en bases de datos
        }

        public void AddMessage(Message message)
        {
            context.Messages.Add(message); 
            // agregar el mensaje en bases de datos
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message); 
            // eliminar el mensaje en bases de datos
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await context.Connections
                .FindAsync(connectionId);
            // traer la conexión mediante el id
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await context.Groups // se hace una consulta a la base de datos group
                .Include(x => x.Connections) // inlcuye las conexiones relacionadas con grupos
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                /*
                    se filtran los grupos mediante la conexión que por 
                    Any: quiere decir que al menos una de las conexiones cumpla la condición
                    en este caso traer el grupo que tenga conexión id igual
                */  
                .FirstOrDefaultAsync(); // trae el primero por defecto
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }
        // traer los mensajes del usuario
        public async Task<PagedList<MessageDto>> GetMessagesForUser(
            MessageParams messageParams
        )
        {
            var query = context.Messages // accedemos a la tabla de mensajes
                .OrderByDescending(x => x.MessageSent) //ordena por orden en desendente fecha de creado el mensaje
                .AsQueryable(); // crea una consulta
            
            query = messageParams.Container switch
            {
                // Inbox = mensajes donde el usuario es el receptor
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username
                && u.RecipientDeleted == false),
                // Outbox= mensajes donde el usuario es el emisor
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
                && u.SenderDeleted == false),
                _ => query
                    .Where(u => u.RecipientUsername == messageParams.Username 
                    && u.RecipientDeleted == false && u.DateRead == null)
            };



            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);
            // la consulta se transforma en un IQueryable<MessageDto> por el mapper

            // crea una paginación para los mensajes
            return await PagedList<MessageDto>
                .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(
            string currentUserName, // usuario actual
            string recipientUserName // usuario recipiente
            ) 
        {
            var query = context.Messages // traer el contexto de la tabla actual
            .Where(m => m.RecipientUsername == currentUserName // traer mensajes si el
            // receptor es el usuario actual
                && m.RecipientDeleted == false // que no esten eliminados
                && m.SenderUsername == recipientUserName // que el usuario que envia los mensajes 
                // es el usuario recipiente
                || m.RecipientUsername == recipientUserName // traer mensajes si el
            // receptor es el usuario recipiente
                && m.SenderDeleted == false // que no este eliminado
                && m.SenderUsername == currentUserName // que el usuario que envia los mensajes 
                // es el usuario actual
                )
            .OrderBy(m => m.MessageSent) // ordena los mensajes por orden de envio o creados
            .AsQueryable(); // crea una consulta

            var unreadMessages = query
                .Where(m => m.DateRead == null && m.RecipientUsername == currentUserName)
                .ToList(); // lista los mensajes del usuario actual que no han sido leidos
            
            if(unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow; // completa el campo de leido de todos
                    // los mensajes del usuario actual que no han sido leidos
                }
            }
            return await query.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
            // lista la consulta y la transforma en MessageDto y la lista

        }

        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection); // elimina la conexión de un usuario
        }
    }
}