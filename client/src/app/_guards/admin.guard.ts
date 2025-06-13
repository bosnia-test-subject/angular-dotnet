/* eslint-disable @typescript-eslint/no-unused-vars */
import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthStoreService } from '../_services/auth-store.service';
import { map, take } from 'rxjs';

export const adminGuard: CanActivateFn = (route, state) => {
  const authStore = inject(AuthStoreService);
  const toastr = inject(ToastrService);

  return authStore.currentUser$.pipe(
    take(1),
    map(user => {
      if (!user || !user.token) {
        toastr.error('You cannot enter this area');
        return false;
      }

      try {
        const decoded = JSON.parse(atob(user.token.split('.')[1]));
        const roles: string[] = Array.isArray(decoded.role)
          ? decoded.role
          : [decoded.role];

        if (roles.includes('Admin') || roles.includes('Moderator')) {
          return true;
        }

        toastr.error('You cannot enter this area');
        return false;
      } catch {
        toastr.error('Invalid token');
        return false;
      }
    })
  );
};
