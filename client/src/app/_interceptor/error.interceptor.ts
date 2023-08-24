import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) {}

  intercept(
    request: HttpRequest<unknown>, //  para obtener la solicitud HTTP entrante
    next: HttpHandler // es un manejador http para continuar el flujo de la solicitud
    ): Observable<HttpEvent<unknown>> { // retorna un observable con la solicitud Http
    return next.handle(request) // continuar el procesamiento de la solicitud http
      .pipe( // pipe es para encadenar una serie de operaciones
      catchError((error: HttpErrorResponse) =>{
        // manejar errores en el procesamiento de datos con status 400 y status 500
        if(error) { // error
          switch(error.status) { // va a evaluar el status del error
            case 400: // status 400
              if(error.error.errors) {
                const modelStateErrors = [];
                for (let key in error.error.errors) { // si hay un array de errores
                  modelStateErrors.push(error.error.errors[key])
                }
                throw modelStateErrors.flat();
                // despues de recopilar los mensajes de error los convierte de un
                // array multidimensional a uno unidimensional
              } else {
                this.toastr.error(error.error, error.status.toString());
                // de lo contario lanza un error general del status
              }
              break;
            case 401: // status 401
              this.toastr.error('Unauthorised', error.status.toString());
              // se lanza el error de Unauthorised
              break;
            case 404: // status 404
              this.router.navigateByUrl('/not-found');
              // va a navegar al link de not-found
              break;
            case 500:
              const navigationExtras: NavigationExtras = {state: {error: error.error}};
              // navigationExtras contiene un estado personalizado y se utiliza para
              // pasar información a la pagina que quiere redirigir
              this.router.navigateByUrl('/server-error', navigationExtras);
              // navega al error del servidor y le pasa información de navigationExtras
              break;
            default:
              this.toastr.error('Something unexpected went wrong');
              // significa algo inesperado acaba de ocurrir
              break;
          }
        }
        throw error;
      })
    );
  }
}
