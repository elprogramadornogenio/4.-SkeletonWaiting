namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers = new();
        /*
            static: la variable OnlineUsrs es compartida por todas las instancias
            readonly: quiere decir que la variable será leida una vez que se determina
            un valor no se puede cambiar es una constante en tiempo de compilación
            diccionary: es un diccionario que generalmente indica que 
            Dictionary<string, List<string>>
            la clave es un nombre de usuario de tipo string
            y la lista son los id de la lista de conexiones

        */

        /*La función de UserConnected que realiza seguimiento de la presencia de los usuarios*/
        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false; 
            /*
                quiere decir que por defecto el usuario no está conectado o si ya tuvo conexiones
                antes
            */ 

            lock(OnlineUsers)
            /*
                el bloque lock garantiza que las operaciones en la variable OnlineUsers se
                realicen de manera segura en un entorno multi-hilo, Esto evita conflictos
                de acceso concurrente
            */
            {
                if (OnlineUsers.ContainsKey(username)) 
                // verifica si ya existe una llave del usuario o si el usuario ya se habia conectado
                {
                    OnlineUsers[username].Add(connectionId); 
                    /*
                        lo único que se hace es agregar el id de la conexión
                    */ 
                }
                else
                {
                    OnlineUsers.Add(username, new List<string>{connectionId});
                    isOnline = true;
                    /*
                        si el usuario no ha tenido conexiones antes crea una llave para el usuario
                        y tambien inicializa la lista con la conexión correspondiente
                        y tambien cambia la variable en true para indicar que tiene una nueva 
                        conexión
                    */
                }
            }

            return Task.FromResult(isOnline); // regresa el valor de la variable

        }

        /*
            UserDisconnected es una funciona que se encarga de administrar los usuario que
            se desconectan de la aplicación
        */
        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false; // quiere decir que el usuario por defecto tiene una conexión

            lock(OnlineUsers)
            /*
                el bloque lock garantiza que las operaciones en la variable OnlineUsers se
                realicen de manera segura en un entorno multi-hilo, Esto evita conflictos
                de acceso concurrente
            */
            {

                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);
                /*
                    si no existe una llave que represente la conexión del usuario
                    quiere decir que el usuario nunca estuvo conectado por lo tanto
                    regresa isOffline = false
                */
                
                OnlineUsers[username].Remove(connectionId); 
                // remueve la conexión de la llave usuario multiples sesiones abiertas

                if (OnlineUsers[username].Count == 0) // si en la llave usuario no hay conexiones
                {
                    OnlineUsers.Remove(username); // remueve la llave usuario

                    isOffline = true; // la variable isOffline queda en true
                }
            }

            return Task.FromResult(isOffline); // regresa el valor booleano de la variable
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers; // se crea una lista de usuario que estan online
            lock(OnlineUsers)
            /*
                el bloque lock garantiza que las operaciones en la variable OnlineUsers se
                realicen de manera segura en un entorno multi-hilo, Esto evita conflictos
                de acceso concurrente
            */
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key)
                    .Select(k => k.Key).ToArray();
                /*
                    ordena los usuario y selecciona la clave para convertirla en un arreglo
                */
            }

            return Task.FromResult(onlineUsers); // retorna la lista de usuarios conectados
        }

        public static Task<List<string>> GetConnectionForUser(string username)
        {
            List<string> connectionIds; // traer la lista de usuarios conectados

            lock (OnlineUsers)
            /*
                el bloque lock garantiza que las operaciones en la variable OnlineUsers se
                realicen de manera segura en un entorno multi-hilo, Esto evita conflictos
                de acceso concurrente
            */
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username); // traer valores de
                // los ids conexiones online
            }

            return Task.FromResult(connectionIds); // retorna los valores de las conexionesIds
        }
    }
}