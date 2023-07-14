import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbDropdownModule, NgbModule, NgbNavModule } from '@ng-bootstrap/ng-bootstrap';
import { ToastrModule } from 'ngx-toastr';
import { GalleryModule, GALLERY_CONFIG, GalleryConfig } from 'ng-gallery';
import { LightboxModule } from 'ng-gallery/lightbox';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    NgbModule,
    NgbDropdownModule,
    NgbNavModule,
    GalleryModule,
    LightboxModule,
    ToastrModule.forRoot({
      positionClass: 'toast-bottom-right'
    })
  ],
  exports: [
    NgbModule,
    NgbDropdownModule,
    NgbNavModule,
    ToastrModule,
    GalleryModule,
    LightboxModule
  ],
  providers: [
    {
      provide: GALLERY_CONFIG,
      useValue: {
        autoHeight: true,
        imageSize: 'cover'
      } as GalleryConfig
    }
  ]
})
export class SharedModule { }
