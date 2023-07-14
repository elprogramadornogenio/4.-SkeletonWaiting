import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { GalleryComponent, ImageItem, GalleryItem, Gallery, ImageSize, ThumbnailsPosition } from 'ng-gallery';
import { Lightbox } from 'ng-gallery/lightbox';

@Component({
  selector: 'app-member-details',
  templateUrl: './member-details.component.html',
  styleUrls: ['./member-details.component.css']
})
export class MemberDetailsComponent implements OnInit {

  member: Member | undefined;
  photos: GalleryItem[] = [];
  active = 1;

  constructor(
    private memberService: MembersService,
    private route: ActivatedRoute,
    public gallery: Gallery,
    public lightbox: Lightbox) {}

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {

    const username = this.route.snapshot.paramMap.get('username');
    if(!username) return;
    this.memberService.getMember(username).subscribe({
      next: member => {
        this.member = member;
        this.getImages();
      }
    });
  }

  getImages() {
    if(!this.member) return;
    this.photos = this.member
      .photos.map( photo => new ImageItem(
        { src: photo.url, thumb: photo.url, alt: this.member?.knownAs }
        ));
    const lightboxRef = this.gallery.ref('lightbox');
    lightboxRef.setConfig({
      imageSize: ImageSize.Cover,
      thumbPosition: ThumbnailsPosition.Top,
    });
    lightboxRef.load(this.photos);
  }

}
