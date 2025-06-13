import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { Observable, of, shareReplay } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { Tag } from '../_models/tag';
import { AuthStoreService } from './auth-store.service';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  private http = inject(HttpClient);
  private authService = inject(AuthStoreService);
  baseUrl = environment.apiUrl;

  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  memberCache = new Map<string, any>();

  private currentUser$?: Observable<User | null>;

  userParams = signal<UserParams | null>(null);

  constructor() {
    this.getCurrentUser().subscribe(user => {
      if (user) {
        this.userParams.set(new UserParams(user));
      }
    });
  }

  getCurrentUser(): Observable<User | null> {
    if (!this.currentUser$) {
      this.currentUser$ = this.authService.currentUser$.pipe(shareReplay(1));
    }
    return this.currentUser$;
  }

  resetUserParams() {
    const user = this.authService.getCurrentUserSnapshot();
    if (user) {
      this.userParams.set(new UserParams(user));
    }
  }

  getUserParams(): UserParams | null {
    return this.userParams();
  }

  setUserParams(params: UserParams) {
    this.userParams.set(params);
  }

  getMembers() {
    const paramsObj = this.userParams();
    if (!paramsObj) return;

    const cacheKey = Object.values(paramsObj).join('-');
    const cachedResponse = this.memberCache.get(cacheKey);

    if (cachedResponse) {
      return setPaginatedResponse(cachedResponse, this.paginatedResult);
    }

    let params = setPaginationHeaders(paramsObj.pageNumber, paramsObj.pageSize);
    params = params.append('minAge', paramsObj.minAge);
    params = params.append('maxAge', paramsObj.maxAge);
    params = params.append('gender', paramsObj.gender);
    params = params.append('orderBy', paramsObj.orderBy);

    return this.http
      .get<Member[]>(this.baseUrl + 'users', { observe: 'response', params })
      .subscribe({
        next: response => {
          setPaginatedResponse(response, this.paginatedResult);
          this.memberCache.set(cacheKey, response);
        },
      });
  }

  getMember(username: string): Observable<Member> {
    const member: Member | undefined = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.body), [])
      .find((m: Member) => m.userName === username);

    if (member) return of(member);

    return this.http
      .get<Member>(`${this.baseUrl}users/${username}`)
      .pipe(shareReplay(1));
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member);
  }

  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {});
  }

  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id);
  }

  getTagsForPhoto(photoId: number) {
    return this.http.get<Tag[]>(this.baseUrl + 'users/tags/' + photoId);
  }

  getPhotosWithTags() {
    return this.http.get<Photo[]>(this.baseUrl + 'users/photos-tags');
  }

  getAllTags() {
    return this.http.get<string[]>(this.baseUrl + 'users/tags');
  }

  addTagToPhoto(photoId: number, tags: string[]) {
    return this.http.post(
      `${this.baseUrl}users/assign-tags/${photoId}`,
      JSON.stringify(tags),
      {
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );
  }

  removeTagFromPhoto(photoId: number, tagName: string) {
    return this.http.delete(
      `${this.baseUrl}users/remove-tag/${photoId}/${tagName}`
    );
  }
}
