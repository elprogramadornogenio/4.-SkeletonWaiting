using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub: Hub
    {
        private readonly PresenceTracker tracker; 
        // inyectar la clase presence tracker

        public PresenceHub(PresenceTracker tracker)
        {
            this.tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var isOnline = await tracker
                .UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            /* 
            pregunta si el usuario está online o ya lo esta para no reportar a los otros usuarios
            */
            if(isOnline) 
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
                /*
                    Clients: Es una función de signalR permite enviar datos a clientes especificos
                    o grupos de clientes que han sido registrados
                
                    Others: Representa a los otros usuario menos al que esta activando esta función

                    SendAsync es un metodo que envia asincronamente el mensaje messages
                */

            var currentUsers = await tracker.GetOnlineUsers(); // traer los usuarios online
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
            /*
                Clients: Es una función de signalR permite enviar datos a clientes especificos
                o grupos de clientes que han sido registrados
                
                Caller: Es el cliente que está en sesión o en contexto

                SendAsync es un metodo que envia asincronamente el mensaje messages
            */
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await tracker
                .UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            // envia el usuario que se desconecta y verifica si hay otras sesiones del mismo usuario

            if(isOffline) // si está offline completamente y no tiene otras sesiones abiertas
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
                // notifica a los otros usuario que se ha desconectado el usuario actual

            await base.OnDisconnectedAsync(exception); 
            // completa la desconexión del usuario desde la clase base
        }

    }
}