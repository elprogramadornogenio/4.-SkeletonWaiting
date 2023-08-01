import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { ImageItem, GalleryItem, Gallery, ImageSize, ThumbnailsPosition } from 'ng-gallery';
import { Lightbox } from 'ng-gallery/lightbox';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';

@Component({
  selector: 'app-member-details',
  templateUrl: './member-details.component.html',
  styleUrls: ['./member-details.component.css']
})
export class MemberDetailsComponent implements OnInit, AfterViewInit {

  @ViewChild('memberTabs')
  memberTabs?: TabsetComponent;
  member: Member | undefined;
  photos: GalleryItem[] = [];
  active = 1;
  messages: Message[] = [];
  activeTab?: TabDirective;

  constructor(
    private memberService: MembersService,
    private route: ActivatedRoute,
    public gallery: Gallery,
    public lightbox: Lightbox,
    private messageService: MessageService) {}



  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => this.member = data['member']
    });


  }

  ngAfterViewInit(): void {
    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
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

  selectTab(heading: string) {
    if(this.memberTabs) {
      this.memberTabs.tabs.find(x => x.heading === heading)!.active = true
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;

    if(this.activeTab.heading === 'Messages') {
      this.loadMessage();
    }
  }

  loadMessage() {
    if(this.member) {
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: messages => this.messages = messages
      });
    }
  }

}
