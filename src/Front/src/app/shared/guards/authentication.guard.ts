import { inject } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { MsalAuthService } from '../services/msal-auth.service';

export const AuthenticationGuard = async (): Promise<boolean | UrlTree> => {
  const msalAuth = inject(MsalAuthService);
  const router = inject(Router);

  try {
    const account = await msalAuth.getActiveAccount();
    if (!account) {
      return router.createUrlTree(['/login']);
    }
    return true;
  } catch (error) {
    // If MSAL initialization or redirect processing fails, ensure we navigate to login
    console.error('AuthenticationGuard: failed to get active account', error);
    return router.createUrlTree(['/login']);
  }
};
