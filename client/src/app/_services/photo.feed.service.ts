import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { interval, Observable } from 'rxjs';
import { switchMap, map, distinctUntilChanged } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root',
})
export class PhotoFeedService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}
  getPhotoFeed(): Observable<Photo[]> {
    return interval(3000).pipe(
      switchMap(() => this.getApprovedPhotos()),
      distinctUntilChanged(
        (prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)
      )
    );
  }

  private getApprovedPhotos(): Observable<Photo[]> {
    return this.http
      .get<Photo[]>(this.baseUrl + 'users/approved-photos')
      .pipe(map(photos => photos.filter(photo => photo.isApproved)));
  }
}
