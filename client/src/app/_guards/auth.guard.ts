/* eslint-disable @typescript-eslint/no-unused-vars */
import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthStoreService } from '../_services/auth-store.service';
import { map, take } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  const authStore = inject(AuthStoreService);
  const toastr = inject(ToastrService);

  return authStore.currentUser$.pipe(
    take(1),
    map(user => {
      if (user) {
        return true;
      } else {
        toastr.error('You shall not pass');
        return false;
      }
    })
  );
};
