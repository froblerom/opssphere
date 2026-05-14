import { CanActivateFn } from '@angular/router';

export const authGuard: CanActivateFn = () => {
  // Future work: evaluate frontend auth state for navigation UX only.
  // Backend authorization remains the source of truth for role and scope access.
  return true;
};
