import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthenticationFacadeService } from '../../shared/facades/authentication.facade.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { MsalAuthService } from '../../shared/services/msal-auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private router = inject(Router);
  private authService = inject(AuthenticationService);
  private authFacade = inject(AuthenticationFacadeService);
  private msalAuthService = inject(MsalAuthService);

  protected email = signal('');
  protected password = signal('');
  protected isLoading = signal(false);
  protected isMsalLoading = signal(false);
  protected errorMessage = signal('');

  protected async loginWithMicrosoft(): Promise<void> {
    this.isMsalLoading.set(true);
    this.errorMessage.set('');
    try {
      await this.msalAuthService.loginPopup();
      await this.router.navigate(['/']);
    } catch {
      this.errorMessage.set('Microsoft authentication failed. Please try again.');
    } finally {
      this.isMsalLoading.set(false);
    }
  }

  protected async loginWithEmail(event: Event): Promise<void> {
    event.preventDefault();
    this.isLoading.set(true);
    this.errorMessage.set('');
    try {
      const result = await this.authFacade.postLogIn$(this.email(), this.password());
      this.authService.setAuthToken(result.token);
      await this.router.navigate(['/']);
    } catch {
      this.errorMessage.set('Invalid email or password. Please try again.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
