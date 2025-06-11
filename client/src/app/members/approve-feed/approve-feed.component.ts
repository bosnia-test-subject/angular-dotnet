import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { PhotoFeedService } from '../../_services/photo.feed.service';
import { Photo } from '../../_models/photo';
import { AuthStoreService } from '../../_services/auth-store.service';

@Component({
  selector: 'app-approve-feed',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './approve-feed.component.html',
  styleUrl: './approve-feed.component.css',
})
export class ApproveFeedComponent implements OnInit {
  photos: Photo[] = [];

  constructor(
    private photoFeedService: PhotoFeedService,
    private authService: AuthStoreService
  ) {}

  ngOnInit(): void {
    this.authService.isLoggedIn$.subscribe(isLoggedIn => {
      if (isLoggedIn) {
        this.photoFeedService.getPhotoFeed().subscribe(data => {
          this.photos = data;
        });
      }
    });
  }
}
