import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import {
  DsPageHeaderComponent,
  DsSectionHeaderComponent,
  DsCardComponent,
  DsButtonComponent,
  DsAlertComponent,
} from '../../shared/components/ds';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { PersonalAccessTokenService } from '../../shared/services/personal-access-token.service';
import { PersonalAccessTokenResponse } from '../../shared/interfaces/personal-access-token.interface';
import { CreatePatDialogComponent } from './create-pat-dialog/create-pat-dialog.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatTooltipModule,
    TranslateModule,
    DsPageHeaderComponent,
    DsSectionHeaderComponent,
    DsCardComponent,
    DsButtonComponent,
    DsAlertComponent,
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent implements OnInit {
  private readonly patService = inject(PersonalAccessTokenService);
  private readonly dialog = inject(MatDialog);

  protected readonly tokens = signal<PersonalAccessTokenResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly errorKey = signal('');

  protected readonly hasTokens = computed(() => this.tokens().length > 0);

  async ngOnInit(): Promise<void> {
    await this.loadTokens();
  }

  private async loadTokens(): Promise<void> {
    this.isLoading.set(true);
    this.errorKey.set('');
    try {
      const result = await this.patService.getAll();
      this.tokens.set(result);
    } catch {
      this.errorKey.set('SETTINGS.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreatePatDialogComponent, {
      width: '520px',
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe(async (created: boolean) => {
      if (created) {
        await this.loadTokens();
      }
    });
  }

  protected openRevokeDialog(token: PersonalAccessTokenResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '440px',
      data: {
        titleKey: 'SETTINGS.REVOKE_CONFIRM_TITLE',
        messageKey: 'SETTINGS.REVOKE_CONFIRM_MESSAGE',
        messageParams: { name: token.name },
        confirmKey: 'SETTINGS.REVOKE',
        cancelKey: 'SETTINGS.CREATE_DIALOG.CANCEL',
      } satisfies ConfirmDialogData,
    });

    dialogRef.afterClosed().subscribe(async (confirmed: boolean) => {
      if (confirmed) {
        await this.revokeToken(token.id);
      }
    });
  }

  private async revokeToken(id: string): Promise<void> {
    this.isLoading.set(true);
    this.errorKey.set('');
    try {
      await this.patService.revoke(id);
      this.tokens.update((tokens) => tokens.filter((t) => t.id !== id));
    } catch {
      this.errorKey.set('SETTINGS.REVOKE_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected getTokenStatus(token: PersonalAccessTokenResponse): 'active' | 'revoked' | 'expired' {
    if (token.isRevoked) {
      return 'revoked';
    }
    if (token.expiresAt && new Date(token.expiresAt) < new Date()) {
      return 'expired';
    }
    return 'active';
  }

  protected getStatusKey(token: PersonalAccessTokenResponse): string {
    const status = this.getTokenStatus(token);
    return `SETTINGS.TABLE.${status.toUpperCase()}`;
  }

  protected formatDate(dateStr: string | null): string {
    if (!dateStr) {
      return '';
    }
    return new Date(dateStr).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }
}
