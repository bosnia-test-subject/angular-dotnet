import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { MessageService } from '../../_services/message.service';
import { PresenceService } from '../../_services/presence.service';
import { HubConnectionState } from '@microsoft/signalr';
import { Subject, takeUntil } from 'rxjs';
import { AuthStoreService } from '../../_services/auth-store.service';
@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [
    TabsModule,
    GalleryModule,
    TimeagoModule,
    DatePipe,
    MemberMessagesComponent,
  ],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css',
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  private presenceService = inject(PresenceService);
  private messageService = inject(MessageService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthStoreService);
  private destroy$ = new Subject<void>();

  member: Member = {} as Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;

  ngOnInit(): void {
    this.route.data.pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.member = data['member'];
        this.member &&
          this.member.photos
            .filter(p => p.isApproved)
            .map(p => {
              this.images.push(new ImageItem({ src: p.url, thumb: p.url }));
            });
      },
    });

    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => this.onRouteParamsChange(),
    });

    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab']);
      },
    });
  }
  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: this.activeTab.heading },
      queryParamsHandling: 'merge',
    });
    if (this.activeTab.heading === 'Messages' && this.member) {
      const user = this.authService.getCurrentUserSnapshot();
      if (!user) return;
      this.messageService.createHubConnection(user, this.member.userName);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  checkOnlineUsers() {
    return this.presenceService.onlineUsers();
  }

  onRouteParamsChange() {
    const user = this.authService.getCurrentUserSnapshot();
    if (!user) return;
    if (
      this.messageService.hubConnection?.state ===
        HubConnectionState.Connected &&
      this.activeTab?.heading === 'Messages'
    ) {
      this.messageService.hubConnection.stop().then(() => {
        this.messageService.createHubConnection(user, this.member.userName);
      });
    }
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      const messageTab = this.memberTabs.tabs.find(x => x.heading === heading);
      if (messageTab) messageTab.active = true;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.messageService.stopHubConnection();
  }
}
