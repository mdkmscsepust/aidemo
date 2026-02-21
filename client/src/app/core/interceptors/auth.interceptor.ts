import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { StorageService } from '../services/storage.service';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const storage = inject(StorageService);
  const authService = inject(AuthService);
  const token = storage.getAccessToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && storage.getRefreshToken() && !req.url.includes('/auth/')) {
        return authService.refreshToken().pipe(
          switchMap(() => {
            const newToken = storage.getAccessToken();
            const retried = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
            return next(retried);
          }),
          catchError(refreshError => throwError(() => refreshError))
        );
      }
      return throwError(() => error);
    })
  );
};
