import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class BusyService {

  busyRequestCount = 0;
  // esta propiedad va a hacer un seguimiento del numero de solicitudes en curso

  constructor(private spinnerService: NgxSpinnerService) { }

  busy() {
    this.busyRequestCount++; // incrementa el contandor en 1 para indicar ocupado
    this.spinnerService.show(undefined, {
      type: 'timer',
      bdColor: 'rgba(255,255,255,0)',
      color: '#333333'
    });
  }

  idle() {
    this.busyRequestCount--;
    // decrementa el contador en 1 para indicar que ha terminado la solicitud
    if(this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.spinnerService.hide(); // oculta el indicador spinnerService
    }
  }

}
