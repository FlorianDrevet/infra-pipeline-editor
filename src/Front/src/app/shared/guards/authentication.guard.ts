import { inject } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { MsalAuthService } from '../services/msal-auth.service';

export const AuthenticationGuard = async (): Promise<boolean | UrlTree> => {
  const msalAuth = inject(MsalAuthService);
  const router = inject(Router);

  const account = await msalAuth.getActiveAccount();
  if (!account) {
    return router.createUrlTree(['/login']);
  }
  return true;
};
