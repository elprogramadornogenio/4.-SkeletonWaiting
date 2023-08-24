import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, take } from 'rxjs';
import { AccountService } from '../_services/account.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(private accountService: AccountService) {}

  intercept(
    request: HttpRequest<unknown>, // toma la petición
    next: HttpHandler): Observable<HttpEvent<unknown>> {

    this.accountService.currentUser$ // Observable<User | null>
    /*
      Abstracción = Es una representación simplificada a que me refiero con esto un ejemplo
      los Observables, como tal representan un flujo de eventos en el interno pero en programación
      se presentan de una forma más simplificada
      Observable = Es una abstracción que se encuentra en muchas bibliotecas de programacion
      incluido RxJS representa una serie de eventos que pueden ser asincronos y emitidos
      a lo largo del tiempo
      Secuencia de eventos
      Asincronia
      Suscripción
      Emisión de Eventos
      Composición
      La analogía del observable es un canal de youtube
      creador de contenido es el observable (observable). Emite continuamente contenido
      suscribes (observando) al canal vas el te notifica de cada vez que yo voy a subir un video
      emisión de eventos: videos, contenido de texto
      desuscribir: ustedes pueden elegir cuando ya no quieren ver contenido de mi canal
    */

    /*
      pipe en RxJs permite utilizar y combinar una seneucna de operaciones para manejar los datos
      se crea para juntar una serie de operaciones en secuencia y ordenadamente
      analogía de los sandwiches en subway el pipe
      analogía de ensablajes de vehiculos el pipe es la linea de emblaje y
      cada estación es una operación
    */
    .pipe(
      take(1)
      /*
        operado take permite tomar uno de los elementos por ejemplo tengo [1,2,3,4]
        pasa con el take(1) = [1]
        pasa por el take(2) = [1,2]
        pasar por el take(3) = [1,2,3]
        pasar por el take(4) = [1,2,3,4]
      */
      ).subscribe({
      next: user => {
        if(user) {
          request = request // representa la solicitud http, contiene la información de la solicitud
            .clone({ // crea una copia de la solicitud original pero con modificaciones
            setHeaders: { // se establece un encabezado personalizado
              Authorization: `Bearer ${user.token}` // se configura un encabezado de autorización
            }
          })
          // finalmente la versión clonada se sobrescribe a la versión original
        }
      }
    });

    return next.handle(request); // se envia al siguiente flujo
  }
}
