import { HttpInterceptorFn } from '@angular/common/http';
import { inject, NgZone } from '@angular/core';
import { LoadingService } from '../_services/loading.service';
import { finalize } from 'rxjs/operators';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  const zone = inject(NgZone);

  console.log('HTTP request intercepted:', req.url);

  zone.run(() => loadingService.incrementRequest());

  return next(req).pipe(
    finalize(() => {
      zone.run(() => loadingService.decrementRequest());
    })
  );
};
