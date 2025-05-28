import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';
import { Photo } from '../_models/photo';
import { Tag } from '../_models/tag';
import { PhotoStats } from '../_models/photoStats';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getTags() {
    return this.http.get<Tag[]>(this.baseUrl + 'admin/get-tags');
  }
  addTag(tag: Tag) {
    return this.http.post(this.baseUrl + 'admin/create-tag', tag);
  }
  removeTagByName(tagName: string) {
    return this.http.delete(this.baseUrl + `admin/remove-tag/${tagName}`, {
      responseType: 'text',
    });
  }
  getPhotoApprovalStats() {
    return this.http.get<PhotoStats[]>(this.baseUrl + 'admin/photo-stats');
  }
  getUsersWithNoMainPhoto() {
    return this.http.get<string[]>(
      this.baseUrl + 'admin/users-without-main-photo'
    );
  }
  getPhotosForApproval() {
    return this.http.get<Photo[]>(this.baseUrl + 'admin/unapproved-photos');
  }
  approvePhoto(id: number) {
    return this.http.post(this.baseUrl + `admin/approve-photo/${id}`, null, {
      responseType: 'text',
    });
  }
  rejectPhoto(id: number) {
    return this.http.delete(this.baseUrl + `admin/reject-photo/${id}`, {
      responseType: 'text',
    });
  }
  getUserWithRoles() {
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }
  updateUserRoles(username: string, roles: string[]) {
    return this.http.post<string[]>(
      this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles,
      { roles: roles }
    );
  }
}
