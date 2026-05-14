import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  // Future work: attach JWT credentials when authentication is implemented.
  // Do not treat frontend token handling or role checks as security enforcement;
  // backend authorization remains the source of truth.
  return next(request);
};
