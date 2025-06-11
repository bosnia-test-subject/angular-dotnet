import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { User } from '../_models/user';
import { environment } from '../../environments/environment';
import { LikesService } from './likes.service';
import { PresenceService } from './presence.service';
import { AuthStoreService } from './auth-store.service';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  private likeService = inject(LikesService);
  private presenceService = inject(PresenceService);
  private authStore = inject(AuthStoreService);

  baseUrl = environment.apiUrl;

  login(model: any): Observable<User> {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map(user => {
        if (user) {
          this.authStore.setCurrentUser(user);
          this.likeService.getLikesIds();
          this.presenceService.createHubConnection(user);
        }
        return user;
      })
    );
  }

  register(model: any): Observable<User> {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          this.authStore.setCurrentUser(user);
          this.likeService.getLikesIds();
          this.presenceService.createHubConnection(user);
        }
        return user;
      })
    );
  }

  logout() {
    this.authStore.clearCurrentUser();
    this.presenceService.stopHubConnection();
  }
}
