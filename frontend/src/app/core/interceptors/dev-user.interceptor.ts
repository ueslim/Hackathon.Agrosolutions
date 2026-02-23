import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const devUserInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const userId = auth.getUserId();
  if (userId) {
    req = req.clone({
      setHeaders: { 'x-dev-user-id': userId },
    });
  }
  return next(req);
};
