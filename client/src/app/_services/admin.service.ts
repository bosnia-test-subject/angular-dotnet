import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  // Task 18. (a)
  getPhotosForApproval() {
    return this.http.get<Photo[]>(this.baseUrl + 'admin/unapproved-photos');
  }
  // Task 18. (b)
  approvePhoto(id: number) {
    return this.http.post(this.baseUrl + `admin/approve-photo/${id}`, null, {
      responseType: 'text',
    });
  }
  // Task 18 (c)
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
      {}
    );
  }
}
