import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { User } from '../_models/user';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class AuthStoreService {
  private readonly storageKey = 'user';

  private currentUserSubject = new BehaviorSubject<User | null>(
    this.loadUserFromStorage()
  );

  public currentUser$ = this.currentUserSubject.asObservable();
  public isLoggedIn$ = this.currentUser$.pipe(map(user => !!user));

  public roles$ = this.currentUser$.pipe(
    map(user => {
      try {
        const payload = user?.token
          ? JSON.parse(atob(user.token.split('.')[1]))
          : null;
        const role = payload?.role;
        return Array.isArray(role) ? role : role ? [role] : [];
      } catch {
        return [];
      }
    })
  );

  public isAdmin$ = this.roles$.pipe(map(roles => roles.includes('Admin')));

  setCurrentUser(user: User) {
    localStorage.setItem(this.storageKey, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  clearCurrentUser() {
    localStorage.removeItem(this.storageKey);
    this.currentUserSubject.next(null);
  }

  getCurrentUserSnapshot(): User | null {
    return this.currentUserSubject.value;
  }

  private loadUserFromStorage(): User | null {
    const json = localStorage.getItem(this.storageKey);
    return json ? JSON.parse(json) : null;
  }
}
