import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MsalAuthService } from '../../shared/services/msal-auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private router = inject(Router);
  private msalAuthService = inject(MsalAuthService);

  protected isMsalLoading = signal(false);
  protected errorMessage = signal('');

  protected async loginWithMicrosoft(): Promise<void> {
    this.isMsalLoading.set(true);
    this.errorMessage.set('');
    try {
      await this.msalAuthService.loginRedirect(`${window.location.origin}/`);
    } catch {
      this.errorMessage.set('Microsoft authentication failed. Please try again.');
      await this.router.navigate(['/login']);
      this.isMsalLoading.set(false);
    }
  }
}
