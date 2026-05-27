import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../shared/services/language.service';

import {
  DsPageHeaderComponent,
  DsSectionHeaderComponent,
  DsCardComponent,
  DsButtonComponent,
  DsAlertComponent,
  DsChipComponent,
  DsTableColumn,
  DsTableComponent,
} from '../../shared/components/ds';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { PersonalAccessTokenService } from '../../shared/services/personal-access-token.service';
import { PersonalAccessTokenResponse } from '../../shared/interfaces/personal-access-token.interface';
import { BicepViewerTheme, AppTheme, UserPreferencesService } from '../../shared/services/user-preferences.service';
import { CreatePatDialogComponent } from './create-pat-dialog/create-pat-dialog.component';

const PAT_TABLE_COLUMN_KEYS = {
  name: 'name',
  tokenPrefix: 'tokenPrefix',
  createdAt: 'createdAt',
  lastUsedAt: 'lastUsedAt',
  expiresAt: 'expiresAt',
  status: 'status',
  actions: 'actions',
} as const;

const PAT_TABLE_DENSITY = 'compact' as const;

const PAT_TABLE_COLUMN_WIDTHS = {
  name: 'minmax(220px, 2fr)',
  tokenPrefix: 'minmax(140px, 1.15fr)',
  createdAt: 'minmax(140px, 1fr)',
  lastUsedAt: 'minmax(220px, 1.35fr)',
  expiresAt: 'minmax(140px, 1fr)',
  status: 'minmax(120px, 0.9fr)',
  actions: 'max-content',
} as const;

const PAT_TABLE_CELL_CLASSES = {
  name: 'pat-table__cell--name',
  lastUsedAt: 'pat-table__cell--datetime',
  actions: 'pat-table__cell--actions',
} as const;

const SETTINGS_LOCALE_BY_LANGUAGE = {
  fr: 'fr-FR',
  en: 'en-US',
} as const;

const DATE_ONLY_FORMAT_OPTIONS = {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  timeZone: 'UTC',
} as const satisfies Intl.DateTimeFormatOptions;

const DATE_TIME_FORMAT_OPTIONS = {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
  timeZone: 'UTC',
} as const satisfies Intl.DateTimeFormatOptions;

const BICEP_THEME_PREVIEW_LINES = [
  {
    id: 'decorator',
    indent: 0,
    segments: [
      { text: '@minLength', tone: 'decorator' },
      { text: '(3)', tone: 'number' },
    ],
  },
  {
    id: 'param',
    indent: 0,
    segments: [
      { text: 'param', tone: 'keyword' },
      { text: ' environment ', tone: 'plain' },
      { text: 'string', tone: 'type' },
      { text: ' = ', tone: 'plain' },
      { text: "'prod'", tone: 'string' },
    ],
  },
  {
    id: 'resource',
    indent: 0,
    segments: [
      { text: 'resource', tone: 'keyword' },
      { text: ' stg ', tone: 'plain' },
      { text: "'Microsoft.Storage/storageAccounts@2024-01-01'", tone: 'string' },
      { text: ' = {', tone: 'plain' },
    ],
  },
  {
    id: 'name',
    indent: 1,
    segments: [
      { text: 'name', tone: 'property' },
      { text: ': ', tone: 'plain' },
      { text: "'st${", tone: 'string' },
      { text: 'uniqueString', tone: 'constant' },
      { text: '(subscription().subscriptionId)}', tone: 'plain' },
      { text: "'", tone: 'string' },
    ],
  },
  {
    id: 'location',
    indent: 1,
    segments: [
      { text: 'location', tone: 'property' },
      { text: ': ', tone: 'plain' },
      { text: 'location', tone: 'plain' },
    ],
  },
  {
    id: 'close',
    indent: 0,
    segments: [{ text: '}', tone: 'plain' }],
  },
] as const;

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
    DsChipComponent,
    DsTableComponent,
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent implements OnInit {
  private readonly patService = inject(PersonalAccessTokenService);
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);
  private readonly userPreferencesService = inject(UserPreferencesService);
  protected readonly languageService = inject(LanguageService);

  protected readonly tokens = signal<PersonalAccessTokenResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly errorKey = signal('');

  protected readonly hasTokens = computed(() => this.tokens().length > 0);
  protected readonly currentLanguage = this.languageService.currentLanguage;
  protected readonly currentBicepViewerTheme = this.userPreferencesService.bicepViewerTheme;
  protected readonly bicepViewerThemeOptions = this.userPreferencesService.bicepViewerThemeOptions;
  protected readonly currentAppTheme = this.userPreferencesService.appTheme;
  protected readonly appThemeOptions = this.userPreferencesService.appThemeOptions;
  protected readonly bicepPreviewLines = BICEP_THEME_PREVIEW_LINES;
  protected readonly patTableDensity = PAT_TABLE_DENSITY;
  protected readonly patTableColumns = computed((): readonly DsTableColumn<PersonalAccessTokenResponse>[] => {
    this.currentLanguage();

    return [
      {
        key: PAT_TABLE_COLUMN_KEYS.name,
        header: this.translate.instant('SETTINGS.TABLE.NAME'),
        width: PAT_TABLE_COLUMN_WIDTHS.name,
        cellClass: PAT_TABLE_CELL_CLASSES.name,
      },
      {
        key: PAT_TABLE_COLUMN_KEYS.tokenPrefix,
        header: this.translate.instant('SETTINGS.TABLE.PREFIX'),
        width: PAT_TABLE_COLUMN_WIDTHS.tokenPrefix,
        mono: true,
      },
      {
        key: PAT_TABLE_COLUMN_KEYS.createdAt,
        header: this.translate.instant('SETTINGS.TABLE.CREATED'),
        width: PAT_TABLE_COLUMN_WIDTHS.createdAt,
      },
      {
        key: PAT_TABLE_COLUMN_KEYS.lastUsedAt,
        header: this.translate.instant('SETTINGS.TABLE.LAST_USED'),
        width: PAT_TABLE_COLUMN_WIDTHS.lastUsedAt,
        cellClass: PAT_TABLE_CELL_CLASSES.lastUsedAt,
      },
      {
        key: PAT_TABLE_COLUMN_KEYS.expiresAt,
        header: this.translate.instant('SETTINGS.TABLE.EXPIRES'),
        width: PAT_TABLE_COLUMN_WIDTHS.expiresAt,
      },
      {
        key: PAT_TABLE_COLUMN_KEYS.status,
        header: this.translate.instant('SETTINGS.TABLE.STATUS'),
        width: PAT_TABLE_COLUMN_WIDTHS.status,
      },
      {
        key: PAT_TABLE_COLUMN_KEYS.actions,
        header: this.translate.instant('SETTINGS.TABLE.ACTIONS'),
        width: PAT_TABLE_COLUMN_WIDTHS.actions,
        align: 'end',
        cellClass: PAT_TABLE_CELL_CLASSES.actions,
      },
    ];
  });

  ngOnInit(): void {
    void this.loadTokens();
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
    return this.formatLocalizedDate(dateStr, DATE_ONLY_FORMAT_OPTIONS);
  }

  protected formatDateTime(dateStr: string | null): string {
    return this.formatLocalizedDate(dateStr, DATE_TIME_FORMAT_OPTIONS);
  }

  private formatLocalizedDate(dateStr: string | null, options: Intl.DateTimeFormatOptions): string {
    if (!dateStr) {
      return '';
    }

    const dateValue = new Date(dateStr);

    if (Number.isNaN(dateValue.getTime())) {
      return '';
    }

    const currentLanguage = this.currentLanguage();
    const locale = SETTINGS_LOCALE_BY_LANGUAGE[currentLanguage]
      ?? SETTINGS_LOCALE_BY_LANGUAGE.fr;

    return new Intl.DateTimeFormat(locale, options).format(dateValue);
  }

  protected selectBicepViewerTheme(theme: BicepViewerTheme): void {
    this.userPreferencesService.setBicepViewerTheme(theme);
  }

  protected selectAppTheme(theme: AppTheme): void {
    this.userPreferencesService.setAppTheme(theme);
  }

  protected trackByTokenId = (_: number, token: PersonalAccessTokenResponse): string => token.id;
}
