import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, delay, finalize } from 'rxjs';
import { BusyService } from '../_services/busy.service';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
    ): Observable<HttpEvent<unknown>> {
    this.busyService.busy(); // mostrar el indicador de cargar
    return next.handle(request)
    /*
      return next.handle(request) permite que next.handle pase al siguiente manejador
      para que el requerimiento siga su curso normal
    */
    .pipe(
      /*
        despues que la solicitud se complete y reciba la respuesta en el servidor se ejecuta
        el pipe
      */
      delay(1000), // el operador delay espera 1 segundo
      finalize(() => {
        /*
          el operador finalize se ejecuta despues de terminada la operación
          el operador finalize es el que se ejecuta de ultimo en el pipe
          analogía de la sopa y el finalize es la etapa de limpiar
        */
        this.busyService.idle();
      })
    );
  }
}
