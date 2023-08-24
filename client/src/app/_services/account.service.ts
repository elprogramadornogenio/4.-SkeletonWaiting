import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';
import { environment } from 'src/environments/environment.development';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  baseUrl = environment.apiUrl; // la url base del servicio
  private currentUserSource = new BehaviorSubject<User | null>(null);
  /*
    new BehaviorSubject<User | null>(null) es un tipo de observable que almacena el ultimo valor
    emitido y permite a los observadores acceder a ese valor cuando se suscriben
  */
  currentUser$ = this.currentUserSource.asObservable();
  /*
    observable<User | null> se va a encargar de observar los cambios del
    new BehaviorSubject<User | null>(null)
    analogía: Altavoz
      El altavoz (BehaviorSubject) el altavoz emite sonidos cada sonido es un evento
      Los asistentes (Observadores) estarán escuchando los ultimos sonidos del altavoz
      Sonidos Emitidos (Valores del BehaviorSubject) el portavoz emite sonidos que representan
      los valores
      Caracteristica del BehaviorSubject Mantiente el ultimo valor emitido para que cualquier
      observador o asistente pueda escuchar el último valor hasta que cambie.
    Diferencias con ReplaySubject: es similar al BehaviorSubject con la diferencia en como
    se ha de manejar los valores pasados
    BehaviorSubject = muestra el valor más reciente a los nuevos observadores
    ReplaySubject = almacena un numero especifico de valores (historial) y los emite a los
    observadores por ejemplo 3 valores los guarda en el buffer y los emite pero son los
    ultimos 3
    tambien el valor inicial
    BehaviorSubject = cuando se crea un behaviorSubject puede proporcinar un valor inicial
    ReplaySubject = No admite valor inicial cuando se crea empieza vacío
    Analogía del ReplaySubject
      La taquilla de un cine (ReplaySubject) digamos que tiene los 3 ultimos estrenos de la semana
      Las personas que van a comprar una boleta (observador) de entrada van a
      escoger de las ultimas 3 peliculas
      la siguiente semana se estrena una pelicula nueva y desplaza una de las tres
      Las peliculas son los valores del replaySubject
      el buffer es la cantidad de peliculas que estan en taquilla.
  */

  constructor(
    private http: HttpClient,
    private presenceService: PresenceService) { }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      /*
        map es uno de los operadores más importantes de la programación reactiva
        y se utiliza en bibliotecas cómo RxJS su proposito es transformar la información
        (recibes unos datos mediante un observable y le cambias un valor)
        analogía
        Te encuentras en un país extranjero donde tu no hablas el idioma local, entonces
        para entenderlos y que ellos te entiendan a ti tu utilizas tu celular (operador map)
        ellos hablan al celular como si fuese un microfono y las palabras que pronuncia
        en el idioma local
        son los valores iniciales (representan elementos del observable) después tu pulsas
        la opción traducir y el celular habla en tu idioma para que tu lo entiendas
        esta representa la transformación que se aplicó a las palabras del idioma local
        para que tu los entendieras.

        tap es un operador de la programación reactiva que se utiliza para hacer operaciones
        secundarias en el flujo de datos, como depuraciones, console log, actualizar alguna
        variable en la aplicación
        analogía
        Guardian del avión
        Hay una ruta donde los pasajeros abordan el avión, los guardias observan a las personas
        y registran sus datos personales el nombre y destino
        sin embargo no pueden modificar la ruta ni la cantidad de personas que ingresan
        solo estan vigilando que todo este en orden
        la ruta del avión (observable)
        los pasajeros que estan en la ruta (valor de los observables)
        los guardias (operador tap)
        la funcionalidad de los guardias tomar datos de los pasajeros (función del tap)

      */
      map((response: User) => {
        const user = response;
        if(user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if(user) {
          this.setCurrentUser(user);
        }
        return user;
      })
    );
  }

  setCurrentUser(user: User) {
    user.roles = [];
    const roles = this.getDecodedToken(user.token).role;
    Array.isArray(roles) ? user.roles = roles: user.roles.push(roles);
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
    this.presenceService.createHubConnection(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.presenceService.stopHubConnection();
  }

  getDecodedToken(token: string) {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
