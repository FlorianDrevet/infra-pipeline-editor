import { Component, HostListener, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { AuthenticationService } from '../../../shared/services/authentication.service';
import { AppLanguage, LanguageService } from '../../../shared/services/language.service';
import { MicrosoftGraphProfilePhotoService } from '../../../shared/services/microsoft-graph-profile-photo.service';
import { MsalAuthService } from '../../../shared/services/msal-auth.service';
import { PageContextService } from '../../../shared/services/page-context.service';
import { DsIconButtonComponent } from '../../../shared/components/ds/ds-icon-button/ds-icon-button.component';
import { SearchDialogComponent } from '../search-dialog/search-dialog.component';

const BlobUrlPrefix = 'blob:';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [MatIconModule, TranslateModule, RouterLink, DsIconButtonComponent],
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.scss',
})
export class NavigationComponent implements OnInit, OnDestroy {
  private readonly authenticationService = inject(AuthenticationService);
  private readonly languageService = inject(LanguageService);
  private readonly microsoftGraphProfilePhotoService = inject(MicrosoftGraphProfilePhotoService);
  private readonly msalAuthService = inject(MsalAuthService);
  private readonly router = inject(Router);
  private readonly pageContextService = inject(PageContextService);
  private readonly dialog = inject(MatDialog);

  protected readonly isLoggingOut = signal(false);
  protected readonly userName = signal('');
  protected readonly userEmail = signal('');
  protected readonly userInitials = signal('IF');
  protected readonly userPhotoUrl = signal<string | null>(null);
  protected readonly currentLanguage = this.languageService.currentLanguage;
  protected readonly availableLanguages = this.languageService.availableLanguages;
  protected readonly breadcrumb = this.pageContextService.breadcrumb;

  public ngOnInit(): void {
    void this.initializeUserProfile();
  }

  public ngOnDestroy(): void {
    this.revokeUserPhotoUrl();
  }

  private async initializeUserProfile(): Promise<void> {
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
    await this.loadUserProfilePhoto();
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

  @HostListener('document:keydown', ['$event'])
  protected onKeydown(event: KeyboardEvent): void {
    if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
      event.preventDefault();
      this.openSearch();
    }
  }

  protected openSearch(): void {
    this.dialog.open(SearchDialogComponent, {
      width: '580px',
      maxHeight: '500px',
      panelClass: 'search-dialog-panel',
      autoFocus: true,
    });
  }

  protected quickCreate(): void {
    void this.router.navigate(['/projects'], { queryParams: { create: true } });
  }

  private async loadUserProfilePhoto(): Promise<void> {
    const userPhotoBlob = await this.microsoftGraphProfilePhotoService.getCurrentUserPhotoBlob();
    if (!userPhotoBlob) {
      this.clearUserPhotoUrl();
      return;
    }

    this.replaceUserPhotoBlob(userPhotoBlob);
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

  private clearUserPhotoUrl(): void {
    this.revokeUserPhotoUrl();
    this.userPhotoUrl.set(null);
  }

  private replaceUserPhotoBlob(userPhotoBlob: Blob): void {
    const userPhotoUrl = globalThis.URL.createObjectURL(userPhotoBlob);
    this.replaceUserPhotoUrl(userPhotoUrl);
  }

  private replaceUserPhotoUrl(userPhotoUrl: string): void {
    this.revokeUserPhotoUrl();
    this.userPhotoUrl.set(userPhotoUrl);
  }

  private revokeUserPhotoUrl(): void {
    const currentUserPhotoUrl = this.userPhotoUrl();
    if (currentUserPhotoUrl?.startsWith(BlobUrlPrefix)) {
      globalThis.URL.revokeObjectURL(currentUserPhotoUrl);
    }
  }
}
