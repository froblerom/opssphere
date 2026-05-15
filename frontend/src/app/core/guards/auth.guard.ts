import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';

import { AuthService } from '../auth/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.getAccessToken()) {
    return true;
  }

  return router.createUrlTree(['/login'], {
    queryParams: {
      returnUrl: state.url
    }
  });
};

export const permissionGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.getAccessToken()) {
    return router.createUrlTree(['/login'], {
      queryParams: {
        returnUrl: state.url
      }
    });
  }

  const requiredPermissions = route.data['permissions'] as string[] | undefined;
  const requiredRoles = route.data['roles'] as string[] | undefined;

  const isAllowed = () => {
    const permissionsAllowed =
      !requiredPermissions?.length || authService.hasAnyPermission(requiredPermissions);
    const rolesAllowed = !requiredRoles?.length || authService.hasAnyRole(requiredRoles);

    return permissionsAllowed && rolesAllowed;
  };

  if (authService.currentProfile()) {
    return isAllowed() ? true : router.createUrlTree(['/dashboard']);
  }

  return authService.me().pipe(
    map(() => (isAllowed() ? true : router.createUrlTree(['/dashboard']))),
    catchError(() => of(router.createUrlTree(['/login'], {
      queryParams: {
        returnUrl: state.url
      }
    })))
  );
};
