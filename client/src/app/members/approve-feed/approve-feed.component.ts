import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { PhotoFeedService } from '../../_services/photo.feed.service';
import { Photo } from '../../_models/photo';
import { AuthStoreService } from '../../_services/auth-store.service';
import { switchMap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';

@Component({
  selector: 'app-approve-feed',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './approve-feed.component.html',
  styleUrl: './approve-feed.component.css',
})
export class ApproveFeedComponent {
  photoFeed$: Observable<Photo[]>;

  constructor(
    private photoFeedService: PhotoFeedService,
    private authService: AuthStoreService
  ) {
    this.photoFeed$ = this.authService.isLoggedIn$.pipe(
      switchMap(isLoggedIn =>
        isLoggedIn ? this.photoFeedService.getPhotoFeed() : of([])
      )
    );
  }
}
