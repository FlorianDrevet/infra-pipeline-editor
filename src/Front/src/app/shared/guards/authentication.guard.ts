import {inject} from '@angular/core';
import {AuthenticationService} from "../services/authentication.service";
import {Router} from "@angular/router";

export const AuthenticationGuard = () => {
  const auth = inject(AuthenticationService);
  const router = inject(Router);

  if (!auth.getIsAuthenticated) {
    router.navigateByUrl('/login').then(null);
    return false
  }
  return true
}
