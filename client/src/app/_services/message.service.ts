import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment.development';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { Message } from '../_models/message';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../_models/user';
import { BehaviorSubject, take } from 'rxjs';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  baseUrl = environment.apiUrl;

  hubUrl = environment.hubUrl;

  private  hubConnection?: HubConnection;

  private messageThreadSource = new BehaviorSubject<Message[]>([]);

  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { }

  createHubConnection(user: User, otherUsername: string) {
    this.hubConnection = new HubConnectionBuilder()
      /*
        Se crea una instancia para configurar el signalR con una url y token de acceso
      */
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      /*
        Se envia como parámetro la variable del otherUsername
      */
      .withAutomaticReconnect()
      /*
        Si se desconecta signalR se va a intentar conectar nuevamente
      */
      .build(); // finalizar a construir la configuración proporcionada.

      this.hubConnection.start().catch(error => console.log(error));
      /*
        Se inicia la conexión con SignalR y cualquier error se captura en consola.
      */

    this.hubConnection.on('ReceiveMessageThread', messages =>{
      /*
        this.hubConnection.on = Se establece una función que va a escuchar al servidor
        cuando se ejecute el evento ReceiveMessageThread y obtiene el mensaje
      */
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      /*
        this.hubConnection.on = Se establece una función que va a escuchar al servidor
        cuando se ejecute el evento UpdatedGroup y obtiene el grupo
      */
      if(group.connection.some(x => x.username === otherUsername)) {
        // si existe un grupo de conexión donde un usuario = al otro usuario
        this.messageThread$.pipe(take(1)).subscribe({
          next: messages => {
            messages.forEach(message =>{
              if(!message.dateRead) {
                message.dateRead = new Date(Date.now()); // deja leido el mensaje
              }
            })
            this.messageThreadSource.next([...messages]); // se actualiza el observador de mensajes
          }
        });


      }
    });

    this.hubConnection.on('NewMessage', message =>{
      /*
        this.hubConnection.on = Se establece una función que va a escuchar al servidor
        cuando se ejecute el evento NewMessage y obtiene el grupo
      */
      this.messageThread$.pipe(take(1)).subscribe({
        next: messages => {
          this.messageThreadSource.next([...messages, message])
          // agregar a messages el ultime message
        }
      });
    });
  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop(); // se desconecta de SignalR
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http);
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }

  async sendMessage(username: string, content: string) {
    return this.hubConnection?.
    invoke('SendMessage', {recipientUsername: username, content})
    /*
      invoke es una funcionalidad que sirve para invocar otro evento en el servidor con
      nombre SendMessage y se le va a enviar recipientUsername y el contenido del mensaje
    */
      .catch(error => console.log(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
