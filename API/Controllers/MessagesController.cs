using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController: BaseApiController
    {
        private readonly IUnitOfWork unitOfWork; // se inyecta la unidad de trabajo
        private readonly IMapper mapper; // se inyecta la función de mapper

        public MessagesController(
            IUnitOfWork unitOfWork,
            IMapper mapper
            )
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(
            CreateMessageDto createMessageDto
            )
        {
            var username = User.GetUsername(); // usuario que está autenticado actualmente

            if(username == createMessageDto.RecipientUsername.ToLower()) 
            // si el usuario que se desea enviar el mensaje es el mismo que lo recibe
                return BadRequest("You cannot send messages to yourself");
                // envia un error 400
            
            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            // traer los datos del usuario que se va a enviar el mensaje (emisor)

            var recipient = await unitOfWork.UserRepository
            .GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            // traer los datos del usuario que se desea enviar el mensaje (receptor)

            if(recipient == null) return NotFound(); 
            // si no se encuentra el receptor del mensaje envia un mensaje 404 

            var message = new Message
            {
                Sender = sender, // se guarda el usuario que envia el mensaje (emisor)
                Recipient = recipient, // se guarda el usuario que recibe el mensaje (receptor)
                SenderUsername = sender.UserName, // usuario que envia el mensaje
                RecipientUsername = recipient.UserName, // usuario que recibe el mensaje
                Content = createMessageDto.Content // Contenido del mensaje
            };

            unitOfWork.MessageRepository.AddMessage(message); // agrega el mensaje

            if(await unitOfWork.Complete()) // si se ha guardado correctamente el mensaje 
                return Ok(mapper.Map<MessageDto>(message));  // returna un status 201

            return BadRequest("Failed to send message"); // de lo contrario falla con un status 400

        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> 
            GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername(); // traer el usuario en sessión
            
            var messages = await unitOfWork.
            MessageRepository.GetMessagesForUser(messageParams); // trae los mensajes que
            // estan pendientes

            Response.AddPaginationHeader(
                new PaginationHeader(
                    messages.CurrentPage, 
                    messages.PageSize, 
                    messages.TotalCount,
                    messages.TotalPages));
            
            return messages; // retorna los mensajes

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id) // se obtiene el id del mensaje
        {
            var username = User.GetUsername(); // traer el usuario en sesión

            var message = await unitOfWork.MessageRepository.GetMessage(id); 
            // trae el id del mensaje a eliminar

            if (
                message.SenderUsername != username 
                // si el usuario que envio el mensaje es diferente al usuario en sesión
                && 
                message.RecipientUsername != username 
                // si el usuario que recibio el mensaje es diferente al usuario en sesión
                )
                return Unauthorized(); // retorna un error 401 de no autorizado
            
            if(message.SenderUsername == username) message.SenderDeleted = true;
            // si el usuario que envio el mensaje ha eliminado el mensaje declara la variable en true
            
            if(message.RecipientUsername == username) message.RecipientDeleted = true;
            // si el usuario que recibio el mensaje ha elminado el mensaje declara la variable en true

            if(message.SenderDeleted && message.RecipientDeleted) 
            // si el mensaje ha sido eliminado por el emisor y recpetor del mensaje el mensaje se elimina
                unitOfWork.MessageRepository.DeleteMessage(message);
            
            if(await unitOfWork.Complete()) return Ok();
            // si se ha actualizado correctamente la base de datos retorna un status 200

            return BadRequest("Problem deleting the message"); // de lo contrario retorna un status 400
        }


    }
}