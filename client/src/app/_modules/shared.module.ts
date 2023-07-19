import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbDropdownModule, NgbModule, NgbNavModule } from '@ng-bootstrap/ng-bootstrap';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { ToastrModule } from 'ngx-toastr';
import { GalleryModule, GALLERY_CONFIG, GalleryConfig } from 'ng-gallery';
import { LightboxModule } from 'ng-gallery/lightbox';
import { NgxSpinnerModule } from 'ngx-spinner';
import { FileUploadModule } from 'ng2-file-upload';

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
    }),
    NgxSpinnerModule.forRoot({
      type: 'timer'
    }),
    BsDatepickerModule.forRoot(),
    FileUploadModule
  ],
  exports: [
    NgbModule,
    NgbDropdownModule,
    NgbNavModule,
    ToastrModule,
    GalleryModule,
    LightboxModule,
    NgxSpinnerModule,
    BsDatepickerModule,
    FileUploadModule
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
