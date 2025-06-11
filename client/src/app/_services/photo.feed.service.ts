import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { interval, Observable } from 'rxjs';
import {
  switchMap,
  map,
  distinctUntilChanged,
  shareReplay,
} from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root',
})
export class PhotoFeedService {
  baseUrl = environment.apiUrl;
  private photoFeed$: Observable<Photo[]>;

  constructor(private http: HttpClient) {
    this.photoFeed$ = interval(10000).pipe(
      switchMap(() => this.getApprovedPhotos()),
      distinctUntilChanged(
        (prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)
      ),
      shareReplay(1)
    );
  }

  getPhotoFeed(): Observable<Photo[]> {
    return this.photoFeed$;
  }

  private getApprovedPhotos(): Observable<Photo[]> {
    return this.http.get<Photo[]>(this.baseUrl + 'users/approved-photos').pipe(
      map(photos => {
        console.log('API response:', photos);
        return photos.filter(photo => photo.isApproved);
      })
    );
  }
}
