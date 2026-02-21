import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 0) {
        snackBar.open('Cannot connect to server. Please try again later.', 'Close', { duration: 5000 });
      } else if (error.status >= 500) {
        snackBar.open('Server error. Please try again later.', 'Close', { duration: 5000 });
      }
      return throwError(() => error);
    })
  );
};
