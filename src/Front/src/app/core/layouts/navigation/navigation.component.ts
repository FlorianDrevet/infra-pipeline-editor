import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { AuthenticationService } from '../../../shared/services/authentication.service';
import { AppLanguage, LanguageService } from '../../../shared/services/language.service';
import { MsalAuthService } from '../../../shared/services/msal-auth.service';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule, TranslateModule],
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.scss',
})
export class NavigationComponent implements OnInit {
  private readonly authenticationService = inject(AuthenticationService);
  private readonly languageService = inject(LanguageService);
  private readonly msalAuthService = inject(MsalAuthService);
  private readonly router = inject(Router);

  protected readonly isLoggingOut = signal(false);
  protected readonly userName = signal('');
  protected readonly userEmail = signal('');
  protected readonly userInitials = signal('IF');
  protected readonly currentLanguage = this.languageService.currentLanguage;
  protected readonly availableLanguages = this.languageService.availableLanguages;

  public async ngOnInit(): Promise<void> {
    const account =
      this.authenticationService.getMsalAccount ??
      (await this.msalAuthService.getActiveAccount());

    if (!account) {
      return;
    }

    const displayName = account.name?.trim() || 'Workspace member';
    const email = account.username?.trim() || 'Connected with Microsoft Entra ID';

    this.userName.set(displayName);
    this.userEmail.set(email);
    this.userInitials.set(this.buildInitials(displayName, email));
  }

  protected async logout(): Promise<void> {
    if (this.isLoggingOut()) {
      return;
    }

    this.isLoggingOut.set(true);

    try {
      await this.msalAuthService.logout();
    } finally {
      this.isLoggingOut.set(false);
      await this.router.navigate(['/login']);
    }
  }

  protected changeLanguage(language: AppLanguage): void {
    this.languageService.setLanguage(language);
  }

  private buildInitials(displayName: string, email: string): string {
    const nameParts = displayName
      .split(' ')
      .map((part) => part.trim())
      .filter((part) => part.length > 0)
      .slice(0, 2);

    if (nameParts.length > 0) {
      return nameParts.map((part) => part[0]?.toUpperCase() ?? '').join('');
    }

    return email.slice(0, 2).toUpperCase() || 'IF';
  }
}
