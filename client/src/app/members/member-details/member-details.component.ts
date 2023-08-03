import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { ImageItem, GalleryItem, Gallery, ImageSize, ThumbnailsPosition } from 'ng-gallery';
import { Lightbox } from 'ng-gallery/lightbox';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { PresenceService } from 'src/app/_services/presence.service';
import { AccountService } from 'src/app/_services/account.service';
import { User } from 'src/app/_models/user';
import { take, delay } from 'rxjs';

@Component({
  selector: 'app-member-details',
  templateUrl: './member-details.component.html',
  styleUrls: ['./member-details.component.css']
})
export class MemberDetailsComponent implements OnInit, AfterViewInit, OnDestroy {

  @ViewChild('memberTabs')
  memberTabs?: TabsetComponent;
  member: Member | undefined;
  photos: GalleryItem[] = [];
  active = 1;
  messages: Message[] = [];
  activeTab?: TabDirective;
  user?: User;

  constructor(
    private accountService: AccountService,
    private route: ActivatedRoute,
    public gallery: Gallery,
    public lightbox: Lightbox,
    private messageService: MessageService,
    public presenceService: PresenceService,
    private router: Router) {
      this.accountService.currentUser$.pipe(take(1)).subscribe({
        next: user => {
          if (user) this.user = user;
        }
      });
    }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }



  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => this.member = data['member']
    });

    this.route.queryParams.pipe(delay(10)).subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    });
  }

  ngAfterViewInit(): void {
    /*
    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    });*/
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

    if(this.activeTab.heading === 'Messages' && this.user && this.member?.userName) {
      this.messageService.createHubConnection(this.user, this.member.userName);
    }
  }

  loadMessage() {
    if(this.member) {
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: messages => this.messages = messages
      });
    } else {
      this.messageService.stopHubConnection();
    }
  }

}
