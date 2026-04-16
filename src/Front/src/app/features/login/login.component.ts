import { Component, inject, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { MsalAuthService } from '../../shared/services/msal-auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly router = inject(Router);
  private readonly msalAuthService = inject(MsalAuthService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  protected isMsalLoading = signal(false);
  protected errorMessageKey = signal('');

  protected async loginWithMicrosoft(): Promise<void> {
    if (!this.isBrowser) return;

    this.isMsalLoading.set(true);
    this.errorMessageKey.set('');
    try {
      await this.msalAuthService.loginRedirect(`${globalThis.location.origin}/`);
    } catch {
      this.errorMessageKey.set('LOGIN.ERROR.MSAL_FAILED');
      await this.router.navigate(['/login']);
      this.isMsalLoading.set(false);
    }
  }
}
