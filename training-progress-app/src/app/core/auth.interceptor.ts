import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ClerkService } from 'ngx-clerk';
import { from, switchMap } from 'rxjs';
import { environment } from '../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  const clerk = inject(ClerkService);
  const session = clerk.session();

  if (!session) {
    return next(req);
  }

  return from(session.getToken()).pipe(
    switchMap((token) => {
      if (!token) {
        return next(req);
      }
      return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
    }),
  );
};
